using System;
using System.Threading.Tasks;
using com.brg.Common;
using Newtonsoft.Json;
using UnityEngine;

namespace com.tinycastle.SeatSeekers
{
    [DataAccessor(typeof(PlayerExtraData))]
    public partial class PlayerExtraDataAccessor : TempJsonSaver<PlayerExtraData>
    {
        private const string SAVE_NAME = "extra_data.json";
        
        public PlayerExtraDataAccessor() : base(Application.persistentDataPath + "/" + SAVE_NAME)
        {
            
        }

        public void AddInterstitialView()
        {
            InterstitialViews += 1;
            WriteDataAsync();
        }
        
        public void AddRewardedView()
        {
            RewardedViews += 1;
            WriteDataAsync();
        }

        public void AddLtv(double value)
        {
            Ltv += value;
            WriteDataAsync();
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
        

        public override Task<bool> WriteDataAsync()
        {
            if (_dto != null)
            {
                _dto.time += UnityGM.PlayTime;
                UnityGM.PlayTime = 0f;
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
    
    public partial class PlayerExtraData
    {
        public double ltv;
        public int interstitialViews;
        public int rewardedViews;
        public float time;
        public DateTime lastModified;
        
        [JsonConstructor]
        public PlayerExtraData()
        {
            ltv = 0f;
            interstitialViews = 0;
            rewardedViews = 0;
            time = 0;
            lastModified = DateTime.UtcNow;
        }
    }
}