using com.brg.Common;
using com.brg.UnityComponents;
using Newtonsoft.Json;

namespace com.tinycastle.SeatSeekers
{
    [ReadableMap(typeof(LevelEntryReader), "Id")]
    [ExposeRead(typeof(GameDataManager))]
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

        public string GetShowUnlockCondition()
        {
            return ShowUnlockCondition == "none" ? DisplayName : ShowUnlockCondition;
        }
    }

    public class LevelEntryReader : AddressableJsonMapReader<LevelEntry>
    {
        public LevelEntryReader() : base("jsons/levels.json")
        {
        }

        public override LevelEntry GetNextItem(string key)
        {
            if (!AllData.TryGetValue(key, out var data))
            {
                LogObj.Default.Warn($"Key \"{key}\" does not exist in LevelEntry");
                return null;
            }

            var order = data.SortOrder + 1;
            var id = $"level_{order}";

            if (!AllData.TryGetValue(id, out var nextData))
            {
                LogObj.Default.Warn($"Level \"{id}\" does not exist in LevelEntry");
                return null;
            }

            return nextData;
        }

        public override LevelEntry GetPrevItem(string key)
        {
            if (!AllData.TryGetValue(key, out var data))
            {
                LogObj.Default.Warn($"Key \"{key}\" does not exist in LevelEntry");
                return null;
            }

            var order = data.SortOrder - 1;
            var id = $"level_{order}";

            if (!AllData.TryGetValue(id, out var prevData))
            {
                LogObj.Default.Warn($"Level \"{id}\" does not exist in LevelEntry");
                return null;
            }

            return prevData;
        }
    }
}