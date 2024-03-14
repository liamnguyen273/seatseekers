using System;

namespace com.brg.Common.Utils
{
    public static partial class Utilities
    {
        public static int Lerp(float ratio, int min, int max)
        {
            return (int)((max - min) * ratio);
        }  
        
        public static float Lerp(float ratio, float min, float max)
        {
            return (max - min) * ratio;
        }
        
        public static float InverseLerp(int value, int min, int max)
        {
            return (value - min) / (float)(max - min);
        }  
        
        public static float InverseLerp(float value, float min, float max)
        {
            return (value - min) / (max - min);
        }

        public static int Clamp(int value, int min, int max)
        {
            return Math.Clamp(value, min, max);
        }
        
        public static float Clamp(float value, float min, float max)
        {
            return Math.Clamp(value, min, max);
        }

        public static float Clamp01(float value)
        {
            return Math.Clamp(value, 0f, 1f);
        }
    }
}