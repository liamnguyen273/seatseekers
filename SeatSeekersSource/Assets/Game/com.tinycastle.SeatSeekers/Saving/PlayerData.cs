using System;
using System.Collections.Generic;
using com.brg.Common;
using Newtonsoft.Json;
using UnityEngine;

namespace com.tinycastle.SeatSeekers
{
    public struct CompletedLevelData
    {
        public bool Completed;
        public int BestTime;

        public CompletedLevelData(bool completed, int bestTime)
        {
            Completed = completed;
            BestTime = bestTime;
        }
    }
    
    [ReadableSingle(typeof(PlayerDataSaver))]
    [WritableSingle(typeof(PlayerDataSaver))]
    [ExposeRead(typeof(GameSaveManager))]
    [ExposeWrite(typeof(GameSaveManager))]
    [Serializable]
    public partial class PlayerData
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, CompletedLevelData> CompletedLevels;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, bool> Ownerships;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, int> Resources;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, int> Leaderboard;
        
        public bool TutorialPlayed { get; set; }
        
        public DateTime LastSaveTime { get; set; }

        [JsonConstructor]
        public PlayerData()
        {
            CompletedLevels = new Dictionary<string, CompletedLevelData>();
            Ownerships = new Dictionary<string, bool>();
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
    }

    public class PlayerDataSaver : JsonFileSingleSaver<PlayerData>
    {
        private const string SAVE_NAME = "player_data.json";
        public PlayerDataSaver() : base(Application.persistentDataPath + "/" + SAVE_NAME)
        {
            
        }
    }
}