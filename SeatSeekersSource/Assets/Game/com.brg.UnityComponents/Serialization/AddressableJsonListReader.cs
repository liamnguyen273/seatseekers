using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using com.brg.Common;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace com.brg.UnityComponents
{
        /// <summary>
    /// Abstract class that reads data from a single JSON file with Unity Addressables containing serialized <typeparamref name="TData"/> instances,
    /// and maintain a list of its data.
    /// </summary>
    /// <remarks>
    /// This reader requires the JSON file exists, and its content deserializable.
    /// <para>Use <see cref="ExposeReadAttribute"/>, <see cref="ReadableListAttribute"/> on
    /// <typeparamref name="TData"/>'s class definition to enable source generation for the
    /// user code assembly's data manager.</para>
    /// </remarks>
    /// <comment>Overrides this class, provides an empty constructor that invokes the constructor to that designated path.</comment>
    /// <typeparam name="TData">The type of the data class.</typeparam>
    public abstract class AddressableJsonListReader<TData> : IListReader<TData>
    {
        private readonly string _addressableKey;
        private List<TData> _data;
        private bool _runningTask;

        /// <inheritdoc/>
        public List<TData> AllData => _data ?? new List<TData>();

        /// <summary>
        /// Creates a Json file list reader that reads from <paramref name="addressableKey"/>.
        /// </summary>
        /// <param name="addressableKey">The path to reads file from.</param>
        protected AddressableJsonListReader(string addressableKey)
        {
            _addressableKey = addressableKey;
            _runningTask = false;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// On WebGL platform, do not use this method. Instead, use <see cref="ReadData(Action&lt;bool&gt;)"/>.
        /// </remarks>
        public async Task<bool> ReadDataAsync()
        {
#if UNITY_WEBGL
            LogObj.Default.Error("AddressableJsonListReader.ReadDataAsync cannot be used on WebGL platform, use ReadData(Action<boo>) instead.");
            return false;
#else
            var oldData = _data;
            if (_runningTask) return false;

            bool result;
            _runningTask = true;
            var handle = Addressables.LoadAssetAsync<TextAsset>(_addressableKey);
            
            try
            {
                var task = handle.Task;
                var json = (await task).text;
                var data = await Task.Run(() => JsonConvert.DeserializeObject<List<TData>>(json));
                
                _data = data ?? throw new Exception("Parsed json is null, this is not allowed");
                _runningTask = false;
                result = true;
            }
            catch (Exception e)
            {
                LogObj.Default.Warn(
                    $"AddressableJsonListReader cannot read data at \"{_addressableKey}\". Exception: {e}");
                _runningTask = false;
                _data = oldData;
                result = false;
            }

            Addressables.Release(handle);
            return result;
#endif
        }

        /// <inheritdoc/>
        /// <remarks>
        /// On WebGL platform, do not use this method. Instead, use <see cref="ReadData(Action&lt;bool&gt;)"/>.
        /// </remarks>
        public bool ReadData()
        {
#if UNITY_WEBGL
            LogObj.Default.Error("AddressableJsonListReader.ReadData cannot be used on WebGL platform, use the override with complete callback instead.");
            return false;
#else
            if (_runningTask) return false;

            _runningTask = true;
            var handle = Addressables.LoadAssetAsync<TextAsset>(_addressableKey);
            handle.WaitForCompletion();
            var result = OnReadDataSynchronously(handle);
            Addressables.Release(handle);
            return result;
#endif
        }

        private Action<bool> _readCompletedCallback;
        
        /// <summary>
        /// Reads data at the Addressables' address, then invoke <paramref name="onReadCompleted"/> when finished.
        /// </summary>
        /// <remarks>
        /// On WebGL platform, use this method over <see cref="ReadData()"/> or <see cref="ReadDataAsync"/> as they both
        /// are not supported.
        /// </remarks>
        /// <param name="onReadCompleted">Callback to invoke with whether the read operation succeeded.</param>
        public void ReadData(Action<bool> onReadCompleted)
        {
            if (_runningTask)
            {
                onReadCompleted?.Invoke(false);
                return;
            }

            _runningTask = true;
            _readCompletedCallback = onReadCompleted;
            var handle = Addressables.LoadAssetAsync<TextAsset>(_addressableKey);
            handle.Completed += OnReadHandleComplete;
            Addressables.Release(handle);
        }

        private void OnReadHandleComplete(AsyncOperationHandle<TextAsset> handle)
        {
            var result = OnReadDataSynchronously(handle);
            _readCompletedCallback?.Invoke(result);
            _readCompletedCallback = null;
        }

        private bool OnReadDataSynchronously(AsyncOperationHandle<TextAsset> handle)
        {
            var oldData = _data;
            
            try
            {
                if (handle.Status != AsyncOperationStatus.Succeeded)
                {
                    throw new Exception($"AsyncOperation ended with status {handle.Status}");
                }
                
                var json = handle.Result.text;
                var data = JsonConvert.DeserializeObject<List<TData>>(json);

                _data = data ?? throw new Exception("Parsed json is null, this is not allowed");
                
                _runningTask = false;
                return true;
            }
            catch (Exception e)
            {
                LogObj.Default.Warn(
                    $"AddressableJsonListReader cannot read data at \"{_addressableKey}\". Exception: {e}");
                _runningTask = false;
                _data = oldData;
                return false;
            }
        }

        /// <inheritdoc/>
        public TData? GetItem(int index)
        {
            return index >= 0 && index < AllData.Count ? AllData[index] : default;
        }
    }
}