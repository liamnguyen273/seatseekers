using System.IO;
using com.brg.Common.Initialization;
using com.brg.UnityCommon.Data;
using UnityEngine;

namespace com.tinycastle.SeatCinema
{
    public partial class GEditor
    {
        public override ReinitializationPolicy ReInitPolicy => ReinitializationPolicy.NOT_ALLOWED;
        protected override void StartInitializationBehaviour()
        {
            MainGame.Initialize();
            string contents = File.ReadAllText(_csvPath);
            EndInitialize(true);
        }

        protected override void EndInitializationBehaviour()
        {
            MainGame.LoadLevel(new LevelEntry()
            {
                
            });
            MainGame.StartGame();
        }
    }
}