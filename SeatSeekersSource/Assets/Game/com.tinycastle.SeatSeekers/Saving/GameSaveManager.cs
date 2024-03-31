using System.Globalization;
using com.brg.Common;
using com.brg.UnityCommon;

namespace com.tinycastle.SeatSeekers
{
    public partial class GameSaveManager : com.brg.Common.SaveManager, ILanguageCodeQuery, IUserAdQuery
    {
        public bool GetAdSkippability(AdRequestType type)
        {
            return GetPlayerData_Ownerships(GlobalConstants.AD_FREE_PACKAGE) ?? false;
        }

        public string GetSelectedLanguage()
        {
            return GetPlayerPreference_SelectedLanguageCode();
        }

        public string GetDefaultLanguage()
        {
            return CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        }

        public void SetSelectedLanguage(string languageCode)
        {
            SetPlayerPreference_SelectedLanguageCode(languageCode);
            m_PlayerPreferenceReaderWriter.WriteDataAsync();
        }
    }
}