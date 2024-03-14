using System.Collections.Generic;

namespace com.brg.Common.Localization
{
    public struct LocalizableText
    {
        public string? Text;
        public Dictionary<string, string>? Params;

        public LocalizableText DeepClone()
        {
            return new LocalizableText()
            {
                Text = Text,
                Params = Params is not null ? new Dictionary<string, string>(Params) : null
            };
        }

        public LocalizableText ShallowClone()
        {
            return new LocalizableText()
            {
                Text = Text,
                Params = Params
            };
        }

        public static implicit operator LocalizableText(string text)
        {
            return new LocalizableText()
            {
                Text = text,
                Params = null
            };
        }

        public static implicit operator string?(LocalizableText text)
        {
            return text.Text;
        }
    }
}