namespace com.brg.Common.Localization
{
    public class TextLocalizationManager
    {
        private static TextLocalizationManager _instance;

        public static TextLocalizationManager Instance
        {
            get
            {
                return _instance ??= new TextLocalizationManager();
            }
        }

        protected TextLocalizationManager()
        {

        }

        public string Localize(string key)
        {
            return key;
        }
    }
}