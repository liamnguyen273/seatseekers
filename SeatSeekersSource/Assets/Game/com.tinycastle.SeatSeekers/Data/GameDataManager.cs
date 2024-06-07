using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using com.brg.Common;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace com.tinycastle.SeatSeekers
{
    public partial class GameDataManager : com.brg.Common.DataManager
    {
        public const string AVATAR_LABEL = "avatars";
        
        private Dictionary<string, Sprite> _avatars;
        private List<string> _leaderboardNames;
        
        public Sprite GetAvatar(string name)
        {
            if (_avatars == null || !_avatars.ContainsKey(name))
            {
                Log.Warn($"Avatar for \"{name}\" does not exist, returning null.");
                return null;
            }

            return _avatars[name];
        }

        protected override async Task<bool> InitializeBehaviourAsync()
        {
            var result = await base.InitializeBehaviourAsync();

            if (!result) return false;
            
            var avatarHandle = Addressables.LoadAssetsAsync<Sprite>(AVATAR_LABEL, sprite => { });
            await avatarHandle.Task;

            if (avatarHandle.Status != AsyncOperationStatus.Succeeded) return false;
            
            _avatars = avatarHandle.Result.ToDictionary(x => x.name, x => x);
            _leaderboardNames = _avatars.Select(x => x.Key).Where(x => x != "You").ToList();
            return true;
        }

        public List<string> GetLeaderboardNames()
        {
            return _leaderboardNames;
        }
        
        public Dictionary<string, ProductEntry> GetAllProducts()
        {
            return m_ProductEntryReader.AllData;
        }
    }
}