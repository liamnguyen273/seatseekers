using System;
using UnityEngine;

namespace com.brg.UnityCommon
{
    public static partial class Utilities
    {
        public static Vector2 RotateByRad(this Vector2 v, float radian) 
        {
            return new Vector2(
                (float)(v.x * Math.Cos(radian) - v.y * Math.Sin(radian)),
                (float)(v.x * Math.Sin(radian) + v.y * Math.Cos(radian))
            );
        }

        public static Vector2 RotateByDeg(this Vector2 v, float degree)
        {
            return v.RotateByRad(Mathf.Deg2Rad * degree);
        }

        public static float RadRotation(this Vector2 v)
        {
            return (float)Math.Atan2(v.y, v.x);
        }
        
        public static float DegRotation(this Vector2 v)
        {
            return v.RadRotation() * Mathf.Rad2Deg;
        }

        public static Color FromHex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out var color);
            return color;
        }
    }
}