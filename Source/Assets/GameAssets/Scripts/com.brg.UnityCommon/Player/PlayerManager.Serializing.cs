using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using com.brg.Common;
using com.brg.Common.ProgressItem;
using Newtonsoft.Json;
using UnityEngine;

namespace com.brg.UnityCommon.Player
{
    public partial class PlayerManager
    {
        private const string PLAYER_DATA_PATH = "player";
        private const string PREFERENCE_KEY = "preference";

        private string PlayerPath => Path.Combine(Application.persistentDataPath, PLAYER_DATA_PATH);
        
        private bool _loadFlag = false;
        private bool _saveFlagPlayer = false;
        private bool _saveFlagPref = false;
        
        public IProgressItem LoadProgressItem { get; protected set; }
        public IProgressItem SaveProgressItem { get; protected set; }

        public void RequestLoadData(bool replaceEvenIfFailed = false,
            bool savePlayer = true,
            bool saveLevel = true,
            bool savePref = true)
        {
            if (_loadFlag)
            {
                Log.Warn("A load request is active, Cannot request another load.");
            }
            
            _loadFlag = true;
            Log.Info("Load data request started.");

            
            var playerTask = savePlayer ? ReadFromDisk<PlayerData>(PlayerPath) : null;
            var prefTask = savePref ? ReadFromPref<PlayerPreference>(PREFERENCE_KEY) : null;

            var tasks = new List<Task>() { playerTask, prefTask }
                .Where(x => x != null);

            Task.WhenAll(tasks)
                .ContinueWith(_ =>
                {
                    if (playerTask != null)
                    {
                        var (foundPlayer, player) = playerTask.Result;
                        _playerData = foundPlayer || replaceEvenIfFailed ? player : _playerData;
                    }

                    if (prefTask != null)
                    {
                        var (foundPref, preference) = prefTask.Result;
                        _preference = foundPref || replaceEvenIfFailed ? preference : _preference;
                    }
                    
                    _loadFlag = false;
                    
                    Log.Info("Load data request completed.");
                }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public void RequestSaveData(            
            bool savePlayer = true,
            bool savePref = true)
        {
            if (savePlayer) SavePlayerData();
            if (savePref) SavePreferenceData();
        }

        private void SavePlayerData()
        {
            if (_saveFlagPlayer)
            {
                Log.Warn("A Player Data save request is active, Cannot request another save.");
                return;
            }
            
            _saveFlagPlayer = true;
            Log.Info("Player Data save request started.");
            var task = WriteToDisk(PlayerPath, _playerData);
                task.ContinueWith(t =>
                {
                    _saveFlagPlayer = false;
                    Log.Info("Player Data save request completed.");
                }, TaskScheduler.FromCurrentSynchronizationContext());
        }
        
        private void SavePreferenceData()
        {
            if (_saveFlagPref)
            {
                Log.Warn("Preference save request is active, Cannot request another save.");
                return;
            }
            
            _saveFlagPref = true;
            Log.Info("Preference save request started.");
            var task = WriteToPref(PREFERENCE_KEY, _preference);
                task.ContinueWith(t =>
                {
                    _saveFlagPref = false;
                    Log.Info("Preference save request completed.");
                }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private async Task<(bool found, T data)> ReadFromPref<T>(string key) where T : new()
        {
            var str = PlayerPrefs.GetString(key, null);
            return await Task.Run(() =>
            {
                T data;
                if (str != null || str == "")
                {
                    try
                    {
                        data = JsonConvert.DeserializeObject<T>(str);
                        return (data != null, data != null ? data : new T());
                    }
                    catch (Exception e)
                    {
                        Log.Warn(e);
                        Log.Warn($"{typeof(T)} data exists on \"{key}\" in PlayerPrefs but cannot be deserialized.");
                    }
                }
                else
                {
                    Log.Info($"{typeof(T)} data is not found on \"{key}\" in PlayerPrefs. Will return default");
                }

                data = new T();
                return (false, data);
            });
        }
        
        
        private async Task<(bool found, T data)> ReadFromDisk<T>(string path) where T : new()
        {
            if (!File.Exists(path))
            {                
                Log.Info($"File at \"{path}\" does not exist, new data will be generated.");
                return (false, new T());
            }
            
            var json = await File.ReadAllTextAsync(path);
            return await Task.Run(() =>
            {
                try
                {
                    var data = JsonConvert.DeserializeObject<T>(json);
                    return (data != null, data != null ? data : new T());;
                }
                catch (Exception e)
                {
                    Log.Warn(e);
                    Log.Info("Player data exists but cannot be deserialized.");
                }

                return (false, new T());
            });
        }

        private async Task<bool> WriteToPref<T>(string key, T serializableData)
        {
            try
            {
                var json = await Task.Run(() => JsonConvert.SerializeObject(serializableData));
                PlayerPrefs.SetString(key, json);
            }
            catch (Exception e)
            {
                Log.Error(e);
                Log.Warn($"Write content in key ${key} is aborted.");
                return false;
            }

            return true;
        }

        private async Task<bool> WriteToDisk<T>(string path, T serializableData)
        {
            try
            {
                var json = await Task.Run(() => JsonConvert.SerializeObject(serializableData));

                if (File.Exists(path))
                {
                    Log.Info($"File at \"{path}\" exists. Will be over-written.");
                }

                await File.WriteAllTextAsync(path, json);
            }
            catch (Exception e)
            {
                Log.Error(e);
                Log.Warn($"Write content at path \"{path}\" is aborted.");
                return false;
            }

            return true;
        }
    }
}