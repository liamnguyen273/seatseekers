using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using com.brg.Common;
using Newtonsoft.Json;
using UnityEngine;

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

        public bool CheckUnlockedBooster(int currentLevel, string boosterName, out bool shouldIntroduce)
        {
            shouldIntroduce = false;
            var hasAlready = GetFromOwnerships(boosterName + "_unlocked") ?? false;
            if (hasAlready) return true;
            
            switch (boosterName)
            {
                case GlobalConstants.BOOSTER_FREEZE_RESOURCE:
                {
                    if (currentLevel < 3) return false;
                    
                    shouldIntroduce = true;
                    SetInOwnerships(GlobalConstants.BOOSTER_FREEZE_UNLOCKED, true, true);
                    WriteDataAsync();

                    return true;
                }
                case GlobalConstants.BOOSTER_JUMP_RESOURCE:
                {
                    if (currentLevel < 4) return false;
                    
                    shouldIntroduce = true;
                    SetInOwnerships(GlobalConstants.BOOSTER_JUMP_UNLOCKED, true, true);
                    WriteDataAsync();

                    return true;
                }
                case GlobalConstants.BOOSTER_EXPAND_RESOURCE:
                {
                    if (currentLevel < 5) return false;
                    
                    shouldIntroduce = true;
                    SetInOwnerships(GlobalConstants.BOOSTER_EXPAND_UNLOCKED, true, true);
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
    }
    
    public partial class PlayerData
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, bool> completedLevels;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [AccessorNotify]
        public Dictionary<string, bool> ownerships;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [AccessorNotify]
        public Dictionary<string, int> resources;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, int> leaderboard;

        public bool tutorialPlayed;
        [DoNotAccess] public DateTime lastModified;

        [AccessorNotify] public int energyRechargeTimer;

        [JsonConstructor]
        public PlayerData()
        {
            completedLevels = new Dictionary<string, bool>();
            ownerships = new Dictionary<string, bool>();
            leaderboard = new Dictionary<string, int>();
            resources = new Dictionary<string, int>()
            {
                { GlobalConstants.COIN_RESOURCE, 100 },
                { GlobalConstants.GEM_RESOURCE, 10 },
                { GlobalConstants.BOOSTER_FREEZE_RESOURCE, 3 },
                { GlobalConstants.BOOSTER_JUMP_RESOURCE, 3 },
                { GlobalConstants.BOOSTER_EXPAND_RESOURCE, 3 },
                { GlobalConstants.ENERGY_RESOURCE, GlobalConstants.MAX_ENERGY },
                { GlobalConstants.INFINITE_ENERGY_RESOURCE, 0 },
            };
            tutorialPlayed = false;
            lastModified = DateTime.UtcNow;
            energyRechargeTimer = 0;
        }
    }
}