using com.brg.Common.Localization;
using UnityEngine;

namespace com.brg.UnityCommon.UI
{
    public class NumberWatcher : ResourceWatcher
    {
        [Header("Number components")]
        [SerializeField] private TextLocalizer _number;
        [SerializeField] private bool _floatyOnChange;
        
        private void Awake()
        {
            _number.Text = "";
        }

        protected override void OnResourceChange(int newValue, int change)
        {
            base.OnResourceChange(newValue, change);
            
            _number.Text = FormatNumber(newValue);

            if (_floatyOnChange && change != 0)
            {
                GM.Instance.Effects.PlayFloatyText(change > 0 ? $"+{change}" : change.ToString(),
                    transform, Vector3.zero, -0.2f);
            }
        }

        protected virtual string FormatNumber(int value)
        {
            return value.ToString();
        }
    }
}