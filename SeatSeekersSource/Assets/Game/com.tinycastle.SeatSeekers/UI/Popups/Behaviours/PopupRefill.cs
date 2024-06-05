using System;
using com.brg.UnityCommon.Editor;
using com.brg.UnityComponents;
using UnityEngine;

namespace com.tinycastle.SeatSeekers
{
    public class PopupRefill : PopupBehaviour
    {
        [SerializeField] private CompWrapper<TextLocalizer> _timerText;
        [SerializeField] private CompWrapper<UIButton> _adButtonx2;
        [SerializeField] private CompWrapper<UIButton> _xButton = "./Panel/TitleGroup/RightButtonGroup/XButton";

        public float Timer { get; set; } = 30f;
        
        private void Awake()
        {
            if (_xButton.NullableComp != null) _xButton.Comp.OnClicked += () => Popup.Hide();
        }

        protected override void InnateOnShowStart()
        {
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
            if (!(Timer > 0f)) return;
            
            Timer -= Time.unscaledDeltaTime;
            var timespan = TimeSpan.FromSeconds(Timer);
            _timerText.Comp.Text = timespan.ToString(@"mm\:ss");
            if (Timer <= 0f)
            {
                _adButtonx2.SetGOActive(false);
            }
        }

        protected override void InnateOnHideEnd()
        {
            base.InnateOnHideEnd();
        }
    }
}