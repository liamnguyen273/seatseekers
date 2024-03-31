using System.IO;
using com.brg.Common;
using Newtonsoft.Json;
using UnityEngine;

namespace com.tinycastle.SeatSeekers
{
    [ExposeRead(typeof(GameSaveManager))]
    [ExposeWrite(typeof(GameSaveManager))]
    [ReadableSingle(typeof(PlayerPreferenceFileSaver))]
    [WritableSingle(typeof(PlayerPreferenceFileSaver))]
    public partial class PlayerPreference
    {
        private int MusicVolume;
        private int SfxVolume;
        private bool Vibration;
        private string SelectedLanguageCode;

        [JsonConstructor]
        public PlayerPreference()
        {
            MusicVolume = 50;
            SfxVolume = 50;
            Vibration = true;
        }

        public PlayerPreference(PlayerPreference other) : this()
        {
            MusicVolume = other.MusicVolume;
            SfxVolume = other.SfxVolume;
            Vibration = other.Vibration;
        }
    }

    public class PlayerPreferenceFileSaver : JsonFileSingleSaver<PlayerPreference>
    {
        private const string FILE_NAME = "preference.json";
        public PlayerPreferenceFileSaver() : base(Path.Combine(Application.persistentDataPath, FILE_NAME))
        {
            
        }
    }
}