using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using com.brg.Common;
using com.brg.Unity;
using com.brg.UnityComponents;
using com.tinycastle.SeatSeekers;
using Newtonsoft.Json;
using UnityEngine;
using Random = UnityEngine.Random;

namespace com.tinycastle.SeatSeekers
{
    [DataAccessor(typeof(PlayerData))]
    public partial class PlayerDataAccessor : TempJsonSaver<PlayerData>
    {
        private const string SAVE_NAME = "player_data.json";
        
        public PlayerDataAccessor() : base(Application.persistentDataPath + "/" + SAVE_NAME)
        {
            
        }

        public bool HasData => _dto != null;
        
        public override bool HasModifiedData => _modified;

        public override async Task<bool> ReadDataAsync()
        {
            var result = await base.ReadDataAsync();

            if (result)
            {
                var data = GetData();
                _dto = data;
            }

            return result;
        }
        
        public Dictionary<string, int> GetLeaderboards()
        {
            return _dto.leaderboard;
        }

        public int GetCompletedLevelCount()
        {
            return _dto.completedLevels.Count(pair => pair.Value);
        }
        
        public void InitializeLeaderboard()
        {
            if (_dto.leaderboard == null || _dto.leaderboard.Count <= 0)
            {
                InitializeLeaderboardHelper();
                FixPlayerLeaderboard();
            }
            else
            {
                UpdateLeaderboardAfterLaunch();
                FixPlayerLeaderboard();
            }
        }

        public void FixPlayerLeaderboard()
        {
            _dto.leaderboard["You"] = GetFromResources(Constants.COIN_RESOURCE) ?? 0;
        }
        
        public void UpdateLeaderboardAfterMatch(string opponentName, int opponentScore)
        {
            const float baseAddChance = 0.1f;
            const float addChanceIntensifier = 0.05f;
            const int scoreRangeLow = 5;
            const int scoreRangeHigh = 15;

            UpdateLeaderboardHelper(new HashSet<string> { opponentName }, 
                baseAddChance,
                addChanceIntensifier, 
                scoreRangeLow, 
                scoreRangeHigh);
        }

        private void InitializeLeaderboardHelper()
        {
            var data = GM.Instance.Get<GameDataManager>();       
            var names = data.GetLeaderboardNames().OrderBy(x => Random.Range(-1.0f, 1.0f));

            _dto.leaderboard["You"] = 0;

            var score = 52;
            var scoreIncrease = 24;
            var scoreIntensifier = 16;
            foreach (var name in names)
            {
                _dto.leaderboard[name] = score;
                scoreIncrease += Random.Range(0, scoreIntensifier + 1);
                score += scoreIncrease;
            }

            _modified = true;
            WriteDataAsync();
        }
        
        private void UpdateLeaderboardAfterLaunch()
        {
            const float baseAddChance = 0.2f;
            const float addChanceIntensifier = 0.05f;
            const int scoreRangeLow = 10;
            const int scoreRangeHigh = 25;
            
            UpdateLeaderboardHelper(null, 
                baseAddChance,
                addChanceIntensifier, 
                scoreRangeLow, 
                scoreRangeHigh);
        }

        public bool CheckUnlockedBooster(int currentLevel, string boosterName, out bool shouldIntroduce)
        {
            shouldIntroduce = false;
            var hasAlready = GetFromOwnerships(boosterName + "_unlocked") ?? false;
            if (hasAlready) return true;
            
            switch (boosterName)
            {
                case Constants.BOOSTER_FREEZE_RESOURCE:
                {
                    if (currentLevel < 8) return false;
                    
                    shouldIntroduce = true;
                    SetInOwnerships(Constants.BOOSTER_FREEZE_UNLOCKED, true, true);
                    WriteDataAsync();

                    return true;
                }
                case Constants.BOOSTER_JUMP_RESOURCE:
                {
                    if (currentLevel < 12) return false;
                    
                    shouldIntroduce = true;
                    SetInOwnerships(Constants.BOOSTER_JUMP_UNLOCKED, true, true);
                    WriteDataAsync();

                    return true;
                }
                case Constants.BOOSTER_EXPAND_RESOURCE:
                {
                    if (currentLevel < 16) return false;
                    
                    shouldIntroduce = true;
                    SetInOwnerships(Constants.BOOSTER_EXPAND_UNLOCKED, true, true);
                    WriteDataAsync();

                    return true;
                }
                default:
                    return false;
            }
        }

        public override Task<bool> WriteDataAsync()
        {
            if (_dto != null)
            {
                _dto.lastModified = DateTime.UtcNow;
            }
            
            return base.WriteDataAsync();
        }

        public override void SetModified(bool modified)
        {
            _modified = modified;
        }

        public DateTime GetLastModified()
        {
            return _dto.lastModified;
        }
        
        private void UpdateLeaderboardHelper(in HashSet<string> noUpdates, 
            float baseAddChance, 
            float addChanceIntensifier, 
            int scoreRangeLow, 
            int scoreRangeHigh)
        {
            var data = GM.Instance.Get<GameDataManager>();
            
            var addChance = baseAddChance;
            var names = data.GetLeaderboardNames();
            foreach (var name in names)
            {
                if (noUpdates?.Contains(name) ?? false) continue;
                var chance = Random.Range(0f, 1f);
                if (chance > addChance)
                {
                    addChance = Mathf.Min(addChance + addChanceIntensifier, 1f);
                    continue;
                }

                addChance = baseAddChance;
                var score = GetFromLeaderboard(name) ?? 0;
                score += Random.Range(scoreRangeLow, scoreRangeHigh + 1);
                SetInLeaderboard(name, score, true);
            }
            
            WriteDataAsync();
        }
    }
    
    public partial class PlayerData
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [AccessorNotify]
        public Dictionary<string, bool> completedLevels;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] 
        [AccessorNotify]
        public Dictionary<string, int> questProgresses;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [AccessorNotify]
        public Dictionary<string, bool> ownerships;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [AccessorNotify]
        public Dictionary<string, int> resources;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [AccessorNotify]
        public Dictionary<string, int> leaderboard;

        public bool tutorialPlayed;
        [DoNotAccess] public DateTime lastModified;

        [AccessorNotify] public int energyRechargeTimer;
        
        [AccessorNotify] public bool hasIAP;
        [AccessorNotify] public bool hasPlayedMultiplayer;
        [AccessorNotify] public bool hasReturned;

        [AccessorNotify] public DateTime dailyRewardTime;
        [AccessorNotify] public int dailyRewardProgress;

        [JsonConstructor]
        public PlayerData()
        {
            completedLevels = new Dictionary<string, bool>();
            ownerships = new Dictionary<string, bool>();
            leaderboard = new Dictionary<string, int>();
            resources = new Dictionary<string, int>()
            {
                { Constants.COIN_RESOURCE, 100 },
                { Constants.GEM_RESOURCE, 10 },
                { Constants.BOOSTER_FREEZE_RESOURCE, 3 },
                { Constants.BOOSTER_JUMP_RESOURCE, 3 },
                { Constants.BOOSTER_EXPAND_RESOURCE, 3 },
                { Constants.ENERGY_RESOURCE, Constants.MAX_ENERGY },
                { Constants.INFINITE_ENERGY_RESOURCE, 0 },
            };
            questProgresses = QuestManager.GetNewQuestItems();
            
            tutorialPlayed = false;
            lastModified = DateTime.UtcNow;

            energyRechargeTimer = 0;

            hasIAP = false;
            hasPlayedMultiplayer = false;
            hasReturned = false;
            
            dailyRewardTime = DateTime.UtcNow;
            dailyRewardProgress = 0;
        }
    }
}