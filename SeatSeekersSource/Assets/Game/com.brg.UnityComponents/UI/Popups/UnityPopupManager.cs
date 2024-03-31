using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using com.brg.Common;
using com.brg.UnityCommon;
using com.brg.UnityCommon.Editor;
using com.brg.UnityCommon.UI;
using UnityEngine;

namespace com.brg.UnityComponents
{
    [InitializationPolicy(InitializationPolicy.ONLY_ONCE)]
    [ProgressLeniency(ProgressLeniency.REQUIRE_ALL_SUCCEEDED)]
    public class PopupManager : ParentManagerBase
    {
        private readonly List<Popup> _list;
        private readonly Dictionary<Type, Popup> _popupsByType;
        private readonly Dictionary<string, Popup> _popupsByName;
        private readonly List<List<Popup>> _activePopupChains;
        private int _activePopupCount;
        
        public bool HasPopups => _activePopupCount > 0;

        public event EventHandler HasPopupEvent;
        public event EventHandler HasNoPopupsEvent;
        
        public PopupManager(IEnumerable<Popup> popups) : base(popups)
        {
            _list = popups.ToList();
            _popupsByType = new Dictionary<Type, Popup>();
            _popupsByName = new Dictionary<string, Popup>();
            _activePopupChains = new List<List<Popup>>();
            _activePopupCount = 0;
        }

        protected override Task<bool> InitializeBehaviourAsync()
        {
            var z = 1;
            foreach (var popup in _list)
            {
                popup.PopupZ = z;

                var type = popup.Behaviour.GetType();

                if (type != typeof(PopupBehaviour))
                {
                    _popupsByType.Add(type, popup);
                }
                
                _popupsByName.Add(popup.ExplicitName, popup);
                
                ++z;
            }
            
            return base.InitializeBehaviourAsync();
        }
        
        public Popup GetPopup<T>(out T behaviour) where T : PopupBehaviour
        {
            if (!_popupsByType.ContainsKey(typeof(T)))
            {
                behaviour = null;
                return null;
            }

            var popup = _popupsByType[typeof(T)];
            behaviour = popup.Behaviour as T;
            return popup;
        }
        
        public Popup GetPopup<T>(string popupName, out T behaviour) where T : PopupBehaviour
        {
            if (!_popupsByName.ContainsKey(popupName))
            {
                behaviour = null;
                return null;
            }

            var popup = _popupsByName[popupName];
            behaviour = popup.Behaviour as T; // Note: can be null
            return popup;
        }
        
        public Popup GetPopup<T>() where T : PopupBehaviour
        {
            return GetPopup<T>(out _);
        }
                
        public Popup GetPopup(string popupName)
        {
            if (!_popupsByName.ContainsKey(popupName))
            {
                return null;
            }

            var popup = _popupsByName[popupName];
            return popup;
        }

        public T GetPopupBehaviour<T>() where T : PopupBehaviour
        {
            if (!_popupsByType.ContainsKey(typeof(T)))
            {
                return null;
            }

            var popup = _popupsByType[typeof(T)];
            var behaviour = popup.Behaviour as T;
            return behaviour;
        }
        
        public T GetPopupBehaviour<T>(string popupName) where T : PopupBehaviour
        {
            if (!_popupsByName.ContainsKey(popupName))
            {
                return null;
            }

            var popup = _popupsByName[popupName];
            var behaviour = popup.Behaviour as T;
            return behaviour;
        }
        
        public PopupBehaviour GetPopupBehaviour(string popupName)
        {
            if (!_popupsByName.ContainsKey(popupName))
            {
                return null;
            }

            var popup = _popupsByName[popupName];
            var behaviour = popup.Behaviour;
            return behaviour;
        }

        public void HideAllPopups(bool immediately = false, bool forced = false)
        {
            var topPopups = new List<Popup>();
            foreach (var chain in _activePopupChains)
            {
                var popup = chain.Count > 0 ? chain[0] : null;
                if (popup is not null)
                {
                    topPopups.Add(popup);
                }
            }

            foreach (var topPopup in topPopups)
            {
                HidePopup(topPopup, immediately, forced, true);
            }
        }
        
        internal void ShowPopup(Popup popup, bool immediately, Type chainTo)
        {
            if (!VerifyExistence(popup))
            {
                Log.Error("Popup is not managed by this manager, abort showing.");
                return;
            }
            
            if (popup.FunctionallyActive || popup.Transiting)
            {
                Log.Warn("Popup is already shown or transiting, cannot show popup again.");
                return;
            }

            ++_activePopupCount;
            popup.PopupZ = _activePopupCount;
            foreach (var list in _activePopupChains)
            {
                if (list.Count > 0 && list.Last().Behaviour.GetType() == chainTo)
                {
                    Log.Info($"Popup {popup.ExplicitName} chained to list leading with {list[0].ExplicitName}");
                    list.Add(popup);
                    return;
                }
            }

            Log.Info($"Popup {popup.ExplicitName} should show.");
            popup.ManagerShow(immediately);
            _activePopupChains.Add(new List<Popup> { popup });
            
            HasPopupEvent?.Invoke(this, null);
        }

        internal void ShowPopup(Popup popup, bool immediately, string chainTo)
        {
            if (!VerifyExistence(popup))
            {
                Log.Error("Popup is not managed by this manager, abort showing.");
                return;
            }
            
            if (popup.FunctionallyActive || popup.Transiting)
            {
                Log.Warn("Popup is already shown or transiting, cannot show popup again.");
                return;
            }

            ++_activePopupCount;
            popup.PopupZ = _activePopupCount;
            popup.transform.SetAsLastSibling();
            foreach (var list in _activePopupChains)
            {
                if (list.Count > 0 && list.Last().ExplicitName == chainTo)
                {
                    Log.Info($"Popup {popup.ExplicitName} chained to list leading with {list[0].ExplicitName}");
                    list.Add(popup);
                    return;
                }
            }

            Log.Info($"Popup {popup.ExplicitName} should show.");
            popup.ManagerShow(immediately);
            _activePopupChains.Add(new List<Popup> { popup });
            HasPopupEvent?.Invoke(this, null);
        }
        
        internal void HidePopup(Popup popup, bool immediately = false, bool forced = false, bool hideChain = true)
        {
            if (!VerifyExistence(popup))
            {
                Log.Error("Popup is not managed by this manager, abort hiding.");
                return;
            }
            
            if (!popup.FunctionallyActive || popup.Transiting)
            {
                Log.Warn("Popup is already hidden or transiting, cannot hide again.");
                return;
            }
            
            // Find chain
            int chainIndex = -1;
            int popupIndex = -1;
            foreach (var list in _activePopupChains)
            {
                ++chainIndex;
                popupIndex = -1;
                var found = false;
                foreach (var otherPopup in list)
                {
                    ++popupIndex;
                    if (otherPopup == popup)
                    {
                        found = true;
                        break;
                    }
                }

                if (found)
                {
                    break;
                }
            }

            if (chainIndex >= 0 && popupIndex >= 0 && chainIndex < _activePopupChains.Count && popupIndex < _activePopupChains[chainIndex].Count)
            {
                if (hideChain)
                {
                    Log.Info("Hiding the popup chain...");
                    
                    foreach (var pop in _activePopupChains[chainIndex])
                    {
                        Log.Info($"Popup {pop.ExplicitName} should hide.");
                        pop.ManagerHide(immediately);
                    }

                    _activePopupCount -= _activePopupChains[chainIndex].Count;
                    _activePopupChains.RemoveAt(chainIndex);
                }
                else
                {
                    _activePopupChains[chainIndex].RemoveAt(popupIndex);
                    _activePopupCount -= 1;
                                
                    Log.Info($"Popup {popup.ExplicitName} should hide.");
                    popup.ManagerHide(immediately);
                }
            }
            else
            {
                Log.Warn("Popup does not belong to any chain, this is unexpected.");
                RecalculatePopupCount();
                            
                Log.Info($"Popup {popup.ExplicitName} should hide.");
                popup.ManagerHide(immediately);
            }

            if (_activePopupCount == 0)
            {
                Log.Info($"Invoke hidden popup event.");
                HasNoPopupsEvent?.Invoke(this, null);
            }
        }

        private void RecalculatePopupCount()
        {
            var before = _activePopupCount;
            
            var count = 0;
            foreach (var chain in _activePopupChains)
            {
                foreach (var popup in chain)
                {
                    ++count;
                }
            }

            _activePopupCount = count;
        }
        
        private bool VerifyExistence(Popup popup)
        {
            var exist = _popupsByType.ContainsKey(popup.Behaviour.GetType()) ||
                   _popupsByName.ContainsKey(popup.ExplicitName);
            
            return exist;
        }
    }
    
    [InitializationPolicy(InitializationPolicy.ONLY_ONCE)]
    [ProgressLeniency(ProgressLeniency.REQUIRE_ALL_SUCCEEDED)]
    public class UnityPopupManager : UnityComp<PopupManager>
    {
        [SerializeField] private GOWrapper _popupHost;

        private void Awake()
        {
            
        }

        public void AttachComps()
        {
            var popups= ExtractPopups();
            Comp = new PopupManager(popups);
        }

        private IEnumerable<Popup> ExtractPopups()
        {
            return _popupHost.Comp.GetDirectOrderedChildComponents<Popup>();
        }
    }
}