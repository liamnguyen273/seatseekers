using System.Linq;
using com.brg.Common;
using com.brg.Unity;
using TMPro;
using UnityEngine;

namespace com.brg.UnityComponents
{
    [RequireComponent(typeof(TMP_Text))]
    public class TextLocalizer : MonoBehaviour
    {
        private TMP_Text _tmp;
        private LocalizableText _text;
        private string _cachedString;
        
        /// <summary>
        /// The localizable text.
        /// </summary>
        public virtual LocalizableText Text
        {
            set => SetText(value);
        }
        
        /// <summary>
        /// The localized text, if it is not localized, localize first.
        /// </summary>
        public string LocalizedText
        {
            get
            {
                if (_cachedString == string.Empty)
                {
                    RefreshAppearance();
                }

                return _cachedString;
            }
        }
        
        private void Awake()
        {
            _tmp = GetComponent<TMP_Text>();
        }

        private void Start()
        {
            var text = _tmp.text;
            _text = text;
            
            var manager = GM.Instance?.Get<LocalizationManager>();
            if (manager is null) return;
            
            RefreshAppearance();
            manager.LanguageChangeEvent += OnLanguageChange;
        }

        protected virtual void OnEnable()
        {
            var manager = GM.Instance?.Get<LocalizationManager>();
            if (manager is null) return;
                
            manager.LanguageChangeEvent += OnLanguageChange;
        }
        
        protected virtual void OnDisable()
        {
            var manager = GM.Instance?.Get<LocalizationManager>();
            if (manager is null) return;
            
            manager.LanguageChangeEvent -= OnLanguageChange;
        }

        /// <summary>
        /// Get a deep-copy of the underlying <see cref="LocalizableText"/>
        /// </summary>
        public LocalizableText CloneText()
        {
            return (LocalizableText)_text.Clone();
        }
        
        /// <summary>
        /// Set a parameter in the underlying <see cref="LocalizableText"/>. Overrides if already exists.
        /// </summary>
        /// <param name="name">Name of the parameter</param>
        /// <param name="value">Reference to the parameter</param>
        public void SetParameter(string name, object value)
        {
            _text.SetParameter(name, value);
            RefreshAppearance();
        }
        
        /// <summary>
        /// Remove a parameter in the underlying <see cref="LocalizableText"/>.
        /// </summary>
        /// <param name="name">Parameter name to remove</param>
        public void RemoveParameter(string name)
        {
            _text.RemoveParameter(name);
            RefreshAppearance();
        }
        
        private void SetText(LocalizableText text)
        {
            _text = text;
            RefreshAppearance();
        }
        
        private void OnLanguageChange(object? sender, string language)
        {
            if (sender is LocalizationManager manager)
            {
                FullyTranslate(manager);
            }
        }

        private void RefreshAppearance()
        {
            FullyTranslate(GM.Instance?.Get<LocalizationManager>());
        }
        
        private void FullyTranslate(LocalizationManager manager)
        {
            if (_tmp == null) return;
            var translatedText = _text.Key;
            var translated = manager?.Translate(_text.Key, out translatedText) ?? false;
            
            _cachedString = _text.IterateParameters()
                .Aggregate(translatedText, (current, replacement) =>
                {
                    var find = $"{{{replacement.name}}}";
                    return current.Replace(find, replacement.value.ToString());
                });

            _tmp.text = _cachedString;
        }
    }
}