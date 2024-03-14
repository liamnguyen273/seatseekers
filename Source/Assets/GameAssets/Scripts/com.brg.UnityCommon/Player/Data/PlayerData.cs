using System;
using System.Collections.Generic;
using com.brg.UnityCommon;
using Newtonsoft.Json;
using UnityEngine.Internal;

namespace com.brg.UnityCommon.Player
{
    [Serializable]
    public struct CompletedLevelData
    {
        public bool Completed { get; set; }
        public int BestTime { get; set; }
    }
    
    [Serializable]
    public class PlayerData
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, CompletedLevelData> CompletedLevels { get; set; }
        
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public HashSet<string> Ownerships { get; set; }
        
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, int> Resources { get; set; }        
        
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, int> Leaderboard { get; set; }
        
        public bool TutorialPlayed { get; set; }
        
        public DateTime LastSaveTime { get; set; }

        [JsonConstructor]
        public PlayerData()
        {
            CompletedLevels = new Dictionary<string, CompletedLevelData>();
            Ownerships = new HashSet<string>();
            Leaderboard = new Dictionary<string, int>();
            Resources = new Dictionary<string, int>()
            {
                { GlobalConstants.SOFT_CURRENCY_RESOURCE, 0 },
                { GlobalConstants.HARD_CURRENCY_RESOURCE, 0 },
                { GlobalConstants.ENERGY_RESOURCE, 0 },
            };
            
            TutorialPlayed = false;
            LastSaveTime = DateTime.UtcNow;
        }

        public static PlayerData From(SyncPlayerData data)
        {
            throw new NotImplementedException();
        }
    }
}