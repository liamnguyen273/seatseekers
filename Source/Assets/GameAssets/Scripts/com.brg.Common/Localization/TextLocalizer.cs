using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace com.brg.Common.Localization
{
    public class TextLocalizer : MonoBehaviour
    {
        [SerializeField] private TMP_Text _textMeshPro;

        private LocalizableText _localizableText;
        private string _cachedText;

        public LocalizableText Text
        {
            get
            {
                if (_localizableText.Text is null)
                {
                    Initialize();
                }

                return _localizableText;
            }
            set => SetContent(value);
        }

        public string CachedLocalizedText => _cachedText;

        public void SetParams(string paramName, object value)
        {
            _localizableText.Params ??= new Dictionary<string, string>();
            _localizableText.Params[$"{{{paramName}}}"] = value.ToString();

            RefreshAppearance();
        }

        public LocalizableText GetContent(bool deepClone = true)
        {
            if (_localizableText.Text is null)
            {
                Initialize();
            }
            
            return deepClone ? _localizableText.DeepClone() : _localizableText.ShallowClone();
        }

        public void SetContent(LocalizableText content, bool deepClone = true)
        {
            _localizableText = deepClone ? content.DeepClone() : content;
            RefreshAppearance();
        }

        private void Initialize()
        {
            var key = _textMeshPro.text;
            _localizableText.Text = key;
        }

        private void RefreshAppearance()
        {
            var localizedText = TextLocalizationManager.Instance.Localize(_localizableText.Text ?? "");

            if (_localizableText.Params is not null)
            {
                foreach (var pair in _localizableText.Params)
                {
                    localizedText = localizedText.Replace(pair.Key, pair.Value);
                }
            }

            _cachedText = localizedText;
            _textMeshPro.text = _cachedText;
        }
    }
}