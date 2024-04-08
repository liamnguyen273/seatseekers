using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace com.brg.Common
{
    public abstract class TempJsonSaver<TData> : ISingleReader<TData>, ISingleWriter<TData> where TData : new()
    {
        private readonly string _filePath;
        private TData? _data;
        private bool _serializing;
        private bool _deserializing;

        public abstract bool HasModifiedData { get; }

        protected TempJsonSaver(string filePath)
        {
            _filePath = filePath;
            _serializing = _deserializing = false;
        }

        public virtual async Task<bool> ReadDataAsync()
        {
            var oldData = _data;
            if (_serializing || _deserializing) return false;

            _deserializing = true;
            try
            {
                if (!File.Exists(_filePath)) throw new Exception($"File at {_filePath} does not exist.");
                
                var json = await File.ReadAllTextAsync(_filePath);
                var data = await Task.Run(() => JsonConvert.DeserializeObject<TData>(json));

                _data = data ?? throw new Exception("Parsed json is null, this is not allowed");
                _deserializing = false;
                return true;
            }
            catch (Exception e)
            {
                LogObj.Default.Warn($"JsonFileSingleSaver cannot read data at \"{_filePath}\", will initialize " +
                                    $"new data or keep old one. Exception: {e}");

                _data = oldData ?? new();  // Initialize new
                
                // Save file
                var json = await Task.Run(() => JsonConvert.SerializeObject(_data, Formatting.Indented));
                await File.WriteAllTextAsync(_filePath, json);
                
                _deserializing = false;
                return true;
            }
        }

        public abstract void SetModified(bool modified);

        public virtual async Task<bool> WriteDataAsync()
        {
            if (_serializing || _deserializing) return false;
            if (_data is null || !HasModifiedData) return true;

            _serializing = true;
            try
            {
                var json = await Task.Run(() => JsonConvert.SerializeObject(_data, Formatting.Indented));
                await File.WriteAllTextAsync(_filePath, json);
                
                SetModified(false);
                _serializing = false;
                return true;
            }
            catch (Exception e)
            {
                LogObj.Default.Warn($"JsonFileSingleSaver cannot write data to \"{_filePath}\". Exception: {e}");
                
                _serializing = false;
                return false;
            }
        }
        
        public virtual TData? GetData()
        {
            return _data;
        }
    }
}