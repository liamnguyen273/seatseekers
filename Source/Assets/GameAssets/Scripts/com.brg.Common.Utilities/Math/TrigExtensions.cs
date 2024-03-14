using System;

namespace com.brg.Common.Utils
{
    public partial class Utilities
    {
        public static float ToDegree(float radian)
        {
            return radian * 180f / MathF.PI;
        }

        public static float ToRadian(float degrees)
        {
            return degrees * MathF.PI / 180f;
        }
    }
}