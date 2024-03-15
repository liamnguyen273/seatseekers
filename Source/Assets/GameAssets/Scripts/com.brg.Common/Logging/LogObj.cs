using System;
using System.Collections.Generic;
#if USING_UNITY
using UnityEngine;
#endif

namespace com.brg.Common.Logging
{
    public class LogObj
    {
        private static class Logger
        {
            public static void Info(string s)
            {
#if USING_GODOT
                GD.Print(s);
#elif USING_UNITY
                Debug.Log(s);
#else
                Console.WriteLine(s);
#endif
            }

            public static void Warn(string s)
            {
#if USING_GODOT
                GD.Print(s);
#elif USING_UNITY
                Debug.LogWarning(s);
#else
                Console.WriteLine(s);
#endif
            }
            
            public static void Error(string s)
            {
#if USING_GODOT
                GD.Print(s);
#elif USING_UNITY
                Debug.LogError(s);
#else
                Console.WriteLine(s);
#endif
            }
        }
        
        public static readonly LogObj Default = new LogObj();
        
        private string? _name;
        
        public LogObj()
        {
            _name = null;
        }        
        
        public void SetName(string name)
        {
            _name = name;
        }
        
        public LogObj(string name)
        {
            _name = name;
        }
        
        public void Info(object message)
        {
            Logger.Info(GetLog("INFO", string.Empty, message));
        }

        public void Info(string extraName, object message)
        {
            Logger.Info(GetLog("INFO", extraName, message));
        }

        public void Warn(object message)
        {
            Logger.Warn(GetLog("WARN", string.Empty, message));
        }
        public void Warn(string extraName, object message)
        {
            Logger.Warn(GetLog("WARN", extraName, message));
        }

        public void Error(object message)
        {
            Logger.Error(GetLog("ERROR", string.Empty, message));
        }
        
        public void Error(string extraName, object message)
        {
            Logger.Error(GetLog("ERROR", extraName, message));
        }

        private string GetLog(string pad, string extraName, object message)
        {
            return $"(GL)[{pad}]{FormatTime(DateTime.Now)} " +
                   $"{(_name != null ? $"[{_name}] " : "")}" +
                   $"{(extraName != string.Empty ? $"[{extraName}] " : "")}" +
                   $"{message}";
        }

        private static string FormatTime(DateTime time)
        {
            return "[" + time.ToString("yy/MM/dd HH:mm:ss") + "]";
        }
    }
}
