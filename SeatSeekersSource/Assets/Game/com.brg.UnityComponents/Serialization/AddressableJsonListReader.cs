using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using com.brg.Common;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.AddressableAssets;

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
        public async Task<bool> ReadDataAsync()
        {
            var oldData = _data;
            if (_runningTask) return false;

            _runningTask = true;
            try
            {
                var handle = Addressables.LoadAssetAsync<TextAsset>(_addressableKey);
                var task = handle.Task;
                var json = (await task).text;
                var data = await Task.Run(() => JsonConvert.DeserializeObject<List<TData>>(json));

                _data = data ?? throw new Exception("Parsed json is null, this is not allowed");
                _runningTask = false;
                return true;
            }
            catch (Exception e)
            {
                LogObj.Default.Warn($"AddressableJsonListReader cannot read data at \"{_addressableKey}\". Exception: {e}");
                _runningTask = false;
                _data = oldData;
                return false;
            }
        }
        
        /// <inheritdoc/>
        public virtual TData? GetItem(int index)
        {
            return index >= 0 && index < AllData.Count ? AllData[index] : default;
        }
    }
}