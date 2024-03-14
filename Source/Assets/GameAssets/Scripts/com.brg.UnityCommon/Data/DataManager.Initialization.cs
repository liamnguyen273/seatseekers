using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using com.brg.Common.Initialization;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace com.brg.UnityCommon.Data
{
    public partial class DataManager : ManagerBase
    {
        public const string LEVEL_JSON_KEY = "jsons/levels.json";
        public const string PRODUCT_JSON_KEY = "jsons/products.json";
        public const string ICON_LABEL = "icons";
        public const string AVATAR_LABEL = "avatars";
        
        private float _progress = 0f;
        
        public override ReinitializationPolicy ReInitPolicy => ReinitializationPolicy.NOT_ALLOWED;


        protected override void StartInitializationBehaviour()
        {
            InitializeTask().ContinueWith(t => EndInitialize(t.Result),
                TaskScheduler.FromCurrentSynchronizationContext());
        }

        protected override void EndInitializationBehaviour()
        {
            
        }

        private async Task<bool> InitializeTask()
        {
            _progress = 0f;
            try
            {
                // Load products
                var productHandle = Addressables.LoadAssetAsync<TextAsset>(PRODUCT_JSON_KEY);
                // Load level entries
                var levelEntryHandle = Addressables.LoadAssetAsync<TextAsset>(LEVEL_JSON_KEY);
                // Resource icons
                var iconHandle = Addressables.LoadAssetsAsync<Sprite>(ICON_LABEL, sprite => { });
                var avatarHandle = Addressables.LoadAssetsAsync<Sprite>(AVATAR_LABEL, sprite => { });
                
                await Task.WhenAll(productHandle.Task, levelEntryHandle.Task, iconHandle.Task);
            
                var prodText = productHandle.Result.text;
                var levelEntryText = levelEntryHandle.Result.text;
                
                _progress = 0.5f;

                var deserializeProdTask = Task.Run(() => JsonConvert.DeserializeObject<Dictionary<string, ProductEntry>>(prodText)!);
                var levelEntryTask = Task.Run(() => JsonConvert.DeserializeObject<Dictionary<string, LevelEntry>>(levelEntryText)!);
                await Task.WhenAll(deserializeProdTask, levelEntryTask);

                _products = await deserializeProdTask ?? new Dictionary<string, ProductEntry>();
                _levelEntries = await levelEntryTask ?? new Dictionary<string, LevelEntry>();

                _resourceIcons = iconHandle.Result.ToDictionary(x => x.name, x => x);
                _avatars = avatarHandle.Result.ToDictionary(x => x.name, x => x);
                _leaderboardNames = _avatars.Select(x => x.Key).Where(x => x != "You").ToList();
                
                // Order levels
                var task = Task.Run(() => MakeSortedLevels(in _levelEntries));
                
                // Release handles, since deserialized
                Addressables.Release(productHandle);
                Addressables.Release(levelEntryHandle);
                
                (_sortedLevels, _sortedLevelsWithDict) = await task;
                _progress = 1f;

            }
            catch (Exception e)
            {
                Log.Error("Reading important data failed.");
                Log.Error(e);
                _progress = 1f;
                return false;
            }
            
            Log.Info($"Read product list: {_products.Count} products.");
            Log.Info($"Read level entry list: {_levelEntries.Count} entries.");
            
            _progress = 1f;
            return true;
        }
        
        private (Dictionary<string, List<LevelEntry>> byList, Dictionary<string, Dictionary<int, LevelEntry>> byDict)
            MakeSortedLevels(in Dictionary<string, LevelEntry> entries)
        {
            var resList = new Dictionary<string, List<LevelEntry>>();
            var resDict = new Dictionary<string, Dictionary<int, LevelEntry>>();

            // Add
            foreach (var (key, entry) in entries)
            {
                var bundle = entry.Bundle;

                if (!resList.ContainsKey(bundle))
                {
                    resList.Add(bundle, new List<LevelEntry>());
                }

                if (!resDict.ContainsKey(bundle))
                {
                    resDict.Add(bundle, new Dictionary<int, LevelEntry>());
                }
                
                resList[bundle].Add(entry);
                resDict[bundle][entry.SortOrder] = entry;
            }
            
            // Sort list
            foreach (var (bundle, list) in resList)
            {
                list.Sort((a, b) => a.SortOrder - b.SortOrder);
            }

            return (resList, resDict);
        }
    }
}