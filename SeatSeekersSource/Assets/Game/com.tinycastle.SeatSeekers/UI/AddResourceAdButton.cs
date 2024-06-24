using System;
using com.brg.Common;
using com.brg.Unity;
using com.brg.UnityCommon;
using com.brg.UnityCommon.Editor;
using com.brg.UnityComponents;
using UnityEngine;

namespace com.tinycastle.SeatSeekers
{
    public class AddResourceAdButton : MonoBehaviour
    {
        [SerializeField] private string _resourceToAdd = "";
        [SerializeField] private int _addCount = 1;
        
        [SerializeField] private CompWrapper<UIButton> _button = "./";

        private void Awake()
        {
#if AD_DISABLED
            _button.Comp.Interactable = false;
#endif
            _button.Comp.OnClicked += OnAd;
        }

        private void OnEnable()
        {
#if AD_DISABLED
            return;
#endif
        }

        private void OnAd()
        {
            if (_resourceToAdd == Constants.ENERGY_RESOURCE)
            {
                var energyCount = GM.Instance.Get<GameSaveManager>().PlayerData.GetFromResources(_resourceToAdd) ?? 0;
                _button.Comp.Interactable = energyCount < Constants.MAX_ENERGY;

                if (energyCount >= Constants.MAX_ENERGY) return;
            }
            
            GM.Instance.Get<UnityAdManager>().RequestAd(new AdRequest(AdRequestType.REWARD_AD, () =>
            {
                var value = GM.Instance.Get<GameSaveManager>().PlayerData.GetFromResources(_resourceToAdd) ?? 0;
                value += _addCount;

                GM.Instance.Get<GameSaveManager>().PlayerData.SetInResources(_resourceToAdd, value, true);
            }, () =>
            {
                // Do nothing
                var popup = GM.Instance.Get<PopupManager>().GetPopup(out PopupBehaviourGeneric generic);
                generic.SetupAsNotify("Uh oh", "Reward ad is not available at the moment, try again later.");
                popup.Show();
            }));
        }
    }
}