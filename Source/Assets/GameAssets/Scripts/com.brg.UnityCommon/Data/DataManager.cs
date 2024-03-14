using System.Collections.Generic;
using UnityEngine;

namespace com.brg.UnityCommon.Data
{
    public partial class DataManager
    {
        // Entries
        private Dictionary<string, ProductEntry> _products;
        private Dictionary<string, LevelEntry> _levelEntries;
        private List<string> _leaderboardNames;
        private Dictionary<string, Sprite> _avatars;
        private Dictionary<string, Sprite> _resourceIcons;

        private Dictionary<string, List<LevelEntry>> _sortedLevels;
        private Dictionary<string, Dictionary<int, LevelEntry>> _sortedLevelsWithDict;
        
        public LevelEntry GetLevelEntry(string levelName)
        {
            if (!_levelEntries.ContainsKey(levelName))
            {
                return null;
            }
            
            var entry = _levelEntries[levelName];
            return entry;
        }

        public LevelEntry GetPreviousEntry(string levelName)
        {
            if (!_levelEntries.TryGetValue(levelName, out var entry))
            {
                Log.Warn($"Level \"{levelName}\" does not exist, cannot get prev entry, returning null.");
                return null;
            }

            var order = entry!.SortOrder - 1;
            var bundle = entry!.Bundle;

            if (!_sortedLevelsWithDict.TryGetValue(bundle, out var dict))
            {
                Log.Warn($"Bundle \"{bundle}\" does not exist, cannot get prev entry, returning null.");
                return null;
            }
            
            if (!dict.TryGetValue(order, out var prevEntry))
            {
                Log.Warn($"Bundle \"{bundle}\" does not have level with sort order {order}, cannot get prev entry, returning null.");
                return null;
            }

            return prevEntry;
        }

        public LevelEntry GetNextEntry(string levelName)
        {
            if (!_levelEntries.TryGetValue(levelName, out var entry))
            {
                Log.Warn($"Level \"{levelName}\" does not exist, cannot get prev entry, returning null.");
                return null;
            }

            var order = entry!.SortOrder + 1;
            var bundle = entry!.Bundle;

            if (!_sortedLevelsWithDict.TryGetValue(bundle, out var dict))
            {
                Log.Warn($"Bundle \"{bundle}\" does not exist, cannot get prev entry, returning null.");
                return null;
            }
            
            if (!dict.TryGetValue(order, out var nextEntry))
            {
                Log.Warn($"Bundle \"{bundle}\" does not have level with sort order {order}, cannot get next entry, returning null.");
                return null;
            }

            return nextEntry;
        }
        
        public List<LevelEntry> GetSortedLevelEntries(string bundle)
        {
            return _sortedLevels.TryGetValue(bundle, out var list) ? list : null;
        }

        public ProductEntry GetProductEntry(string id)
        {
            if (!_products.ContainsKey(id))
            {
                Log.Warn($"Product \"{id}\" doesn't exist, returning null.");
                return null;
            }
            
            return _products[id];
        }

        public Dictionary<string, ProductEntry> GetAllProducts()
        {
            return _products;
        }

        public Sprite GetResourceIcon(string name)
        {
            if (_resourceIcons?.ContainsKey(name) ?? false) return _resourceIcons[name];
            
            Log.Warn($"Resource icon for \"{name}\" does not exist, returning null.");
            return null;
        }
        
        public Sprite GetAvatar(string name)
        {
            if (_avatars?.ContainsKey(name) ?? false) return _avatars[name];
            
            Log.Warn($"Avatar for \"{name}\" does not exist, returning null.");
            return null;
        }

        public List<string> GetLeaderboardNames()
        {
            return _leaderboardNames;
        }
    }
}