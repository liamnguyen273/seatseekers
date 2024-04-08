using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace com.tinycastle.SeatSeekers
{
    public static class Utils
    {
#if UNITY_EDITOR
        private const string LEVEL_INPUT_PATH = "../Data/";
        private const string LEVEL_INPUT_NAME = "level_inputs";
#else
        private const string LEVEL_INPUT_PATH = "./Data/";
        private const string LEVEL_INPUT_NAME = "level_inputs";
#endif
        public static LevelGen[] ReadLevelGenCsv()
        {
            var path = LEVEL_INPUT_PATH + LEVEL_INPUT_NAME + ".csv";
            var content = File.ReadAllLines(path);
            return ParseCsv(content);
        }

        public static LevelGen[] ParseCsv(string[] content)
        {
            var lineTokens = content.Select(x => x.Split(',')).ToList();
            lineTokens.RemoveAt(0);

            var levels = new List<LevelGen>();
            foreach (var tokens in lineTokens)
            {
                if (tokens.Length != 7)
                {
                    continue;
                }

                var level = new LevelGen();
                level.LevelName = tokens[0];
                int.TryParse(tokens[1], out level.CarW);
                int.TryParse(tokens[2], out level.CarH);
                int.TryParse(tokens[4], out level.GenSeatCount);
                int.TryParse(tokens[5], out level.GenColorCount);
                level.LevelData = LevelData.Make(tokens[6]);
                
                levels.Add(level);
            }

            return levels.ToArray();
        }

        public static void WriteCsvs(LevelGen[] levelGen)
        {
            var filePath = LEVEL_INPUT_PATH + LEVEL_INPUT_NAME + ".csv";
            if (File.Exists(filePath))
            {
                var dateTime = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");
                var backupFilePath = LEVEL_INPUT_PATH + LEVEL_INPUT_NAME + "_BAK_" + dateTime + ".csv";
                
                File.Copy(filePath, backupFilePath);
            }

            var builder = new StringBuilder(); 
            builder.AppendLine("levelName,carW,carH,totalSeatCount,genSeatCount,genColorCount,seatArray");

            foreach (var level in levelGen)
            {
                var line = $"{level.LevelName},{level.CarW},{level.CarH},{level.CarW * level.CarH}," +
                           $"{level.GenSeatCount},{level.GenColorCount},{level.LevelData.MakeString()}";

                builder.AppendLine(line);
            }

            File.WriteAllText(filePath, builder.ToString());
            
            Debug.Log($"Saved data to {filePath}");
        }

        public static string FormatTime(int seconds)
        {
            var m = seconds / 60;
            var s = seconds % 60;
            return $"{m:00}:{s:00}";
        }
    }
}