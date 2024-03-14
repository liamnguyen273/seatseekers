using com.brg.Common;
using com.brg.Common.Localization;
using JSAM;
using Lean.Transition;
using UnityEngine;
using UnityEngine.UI;

namespace com.brg.UnityCommon.UI
{
    public class UIButton : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private TextLocalizer _label;
        [SerializeField] private Image _icon;
        [SerializeField] private LeanPlayer _player;
        
        [Header("Params")]
        [SerializeField] private Sounds _tapSound = Sounds.Button;
        [SerializeField] private EventWrapper _buttonClickedEvent;

        private Button _unityButton;
        
        public LocalizableText Label
        {
            get => _label.Text;
            set
            {
                if (_label != null)
                {
                    _label.Text = value;
                }
            }
        }

        public Sprite Icon
        {
            get => _icon.sprite;
            set
            {
                if (_icon != null)
                {
                    _icon.sprite = value;
                }
            }
        }

        public bool Interactable
        {
            get => _unityButton.interactable;
            set => _unityButton.interactable = value;
        }

        public Button UnityButton => _unityButton;
        public Image IconImage => _icon;

        public EventWrapper Event => _buttonClickedEvent;

        private void Awake()
        {
            _unityButton = GetComponent<Button>();
            _unityButton.onClick.AddListener(OnUnityButtonClick);
        }

        public void OnUnityButtonClick()
        {
            // other things
            AudioManager.PlaySound(_tapSound, gameObject.transform);

            _player.Begin();
            _buttonClickedEvent?.Invoke();
        }
    }
}
