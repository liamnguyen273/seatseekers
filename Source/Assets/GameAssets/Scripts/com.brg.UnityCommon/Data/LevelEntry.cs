using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace com.brg.UnityCommon.Data
{
    [Serializable]
    public class LevelEntry
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("bundle")] public string Bundle { get; set; }
        [JsonProperty("sortOrder")] public int SortOrder { get; set; }
        [JsonProperty("levelName")] public string DisplayName { get; set; }
        [JsonProperty("unlockCondition")] public string UnlockCondition { get; set; }
        [JsonProperty("showUnlockCondition")] public string ShowUnlockCondition { get; set; }
        [JsonProperty("showInGame")] public bool ShowInGame { get; set; }

        [JsonIgnore] public bool Playable { get; internal set; }
        [JsonIgnore] public bool EvaluatedPlayable => Playable;
        [JsonIgnore] public int TotalStickerCount { get; internal set; }

        public string GetShowUnlockCondition()
        {
            return ShowUnlockCondition == "none" ? DisplayName : ShowUnlockCondition;
        }
    }
}