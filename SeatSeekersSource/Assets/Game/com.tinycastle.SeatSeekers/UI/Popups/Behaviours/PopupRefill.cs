using com.brg.UnityCommon.Editor;
using com.brg.UnityComponents;
using UnityEngine;

namespace com.tinycastle.SeatSeekers
{
    public class PopupRefill : PopupBehaviour
    {
        [SerializeField] private CompWrapper<UIButton> _adButton;
        [SerializeField] private CompWrapper<UIButton> _xButton = "./Panel/TitleGroup/RightButtonGroup/XButton";
        
        public float Timer { get; set; }
        
        private void Awake()
        {
            if (_xButton.NullableComp != null) _xButton.Comp.OnClicked += () => Popup.Hide();
        }

        protected override void InnateOnShowStart()
        {
            if (Timer <= 0f)
            {
                _adButton.SetGOActive(false);
            }
            else
            {
                _adButton.SetGOActive(true);
            }
            
            base.InnateOnShowStart();
        }

        private void Update()
        {
            if (Timer > 0f)
            {
                Timer -= Time.unscaledDeltaTime;
                if (Timer <= 0f)
                {
                    _adButton.SetGOActive(false);
                }
            }
        }

        protected override void InnateOnHideEnd()
        {
            Timer = 0f;
            base.InnateOnHideEnd();
        }
    }
}