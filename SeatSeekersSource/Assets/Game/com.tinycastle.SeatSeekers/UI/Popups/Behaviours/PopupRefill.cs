using System;
using com.brg.Unity;
using com.brg.UnityCommon.Editor;
using com.brg.UnityComponents;
using UnityEngine;

namespace com.tinycastle.SeatSeekers
{
    public class PopupRefill : PopupBehaviour
    {
        [SerializeField] private CompWrapper<TextLocalizer> _timerText;
        [SerializeField] private CompWrapper<UIButton> _adButton;
        [SerializeField] private CompWrapper<UIButton> _adButtonx2;
        [SerializeField] private CompWrapper<UIButton> _refillButton;
        [SerializeField] private CompWrapper<UIButton> _xButton = "./Panel/TitleGroup/RightButtonGroup/XButton";

        public float Timer { get; set; } = 30f;
        
        private void Awake()
        {
            if (_xButton.NullableComp != null) _xButton.Comp.OnClicked += () => Popup.Hide();
        }

        protected override void InnateOnShowStart()
        {
            var currHeart = GM.Instance.Get<GameSaveManager>().PlayerData.GetFromResources(Constants.ENERGY_RESOURCE) ?? 0;

            if (currHeart >= Constants.MAX_ENERGY)
            {
                _adButton.Comp.Interactable = false;
                Timer = 0f;
                _adButtonx2.SetGOActive(false);
                _refillButton.Comp.Interactable = false;
            }
            
            if (Timer <= 0f)
            {
                _adButtonx2.SetGOActive(false);
            }
            else
            {
                _adButtonx2.SetGOActive(true);
            }

            var timespan = TimeSpan.FromSeconds(Timer);
            _timerText.Comp.Text = timespan.ToString(@"mm\:ss");
            
            base.InnateOnShowStart();
        }

        private void Update()
        {
            var currHeart = GM.Instance.Get<GameSaveManager>().PlayerData.GetFromResources(Constants.ENERGY_RESOURCE) ??
                            0;

            if (currHeart >= Constants.MAX_ENERGY)
            {
                _adButton.Comp.Interactable = false;
                Timer = 0f;
                _adButtonx2.SetGOActive(false);
                _refillButton.Comp.Interactable = false;
            }
            
            if (Timer <= 0f) return;
            
            Timer -= Time.unscaledDeltaTime;
            var timespan = TimeSpan.FromSeconds(Timer);
            _timerText.Comp.Text = timespan.ToString(@"mm\:ss");
            if (Timer <= 0f)
            {
                _adButtonx2.SetGOActive(false);
            }
        }
    }
}