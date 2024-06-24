using System.Globalization;
using System.Threading.Tasks;
using com.brg.Common;
using com.brg.UnityCommon;

namespace com.tinycastle.SeatSeekers
{
    public partial class GameSaveManager : com.brg.Common.SaveManager, ILanguageCodeQuery, IUserAdQuery
    {
        public PlayerDataAccessor PlayerData { get; }
        public PlayerExtraDataAccessor ExtraData { get; }

        public GameSaveManager(PlayerDataAccessor playerData, PlayerExtraDataAccessor extraData)
        {
            PlayerData = playerData;
            ExtraData = extraData;
        }

        protected override async Task<bool> InitializeBehaviourAsync()
        {
            var readComplete = await PlayerData.ReadDataAsync();
            if (!readComplete) return false;
            return await base.InitializeBehaviourAsync();
        }

        public new void SaveAll()
        {
            base.SaveAll();
            PlayerData.WriteDataAsync();
            ExtraData.WriteDataAsync();
        }

        public void SavePlayerData()
        {
            PlayerData.WriteDataAsync();
        }

        public void SaveExtraData()
        {
            ExtraData.WriteDataAsync();
        }

        public bool GetAdSkippability(AdRequestType type)
        {
            var success = PlayerData.TryGetFromOwnerships(Constants.AD_FREE_PACKAGE, out var has);
            return type != AdRequestType.REWARD_AD && success && has is true;
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