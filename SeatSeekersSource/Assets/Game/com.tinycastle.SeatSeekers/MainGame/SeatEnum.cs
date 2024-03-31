using UnityEngine;

namespace com.tinycastle.SeatSeekers
{
    public enum SeatEnum
    {
        // Empty seat
        NONE = 0,
        
        // Seats
        ANY = 1,
        
        BLUE = 2,
        GREEN = 3,
        RED = 4,
        MAGENTA = 5,
        YELLOW = 6,
        CYAN = 7,
        PURPLE = 8,
        ORANGE = 9,
        TEAL = 10,
        BROWN = 11,
        
        // All other numbers from 11-99 are obstacles
    }

    public static class SeatColorUtils
    {
        public static Color GetColor(SeatEnum seatType)
        {
            return GetColor((int)seatType % SeatData.DOUBLE_SEAT_LEFT);
        }
        
        public static Color GetColor(int seatType)
        {
            return (seatType) switch
            {
                (int)SeatEnum.NONE => Color.gray,
                (int)SeatEnum.ANY => Color.gray,
                (int)SeatEnum.BLUE => brg.UnityCommon.Utils.FromHex("#5398f6"),
                (int)SeatEnum.GREEN => brg.UnityCommon.Utils.FromHex("#75ee5d"),
                (int)SeatEnum.RED => brg.UnityCommon.Utils.FromHex("#f3595d"),
                (int)SeatEnum.MAGENTA => brg.UnityCommon.Utils.FromHex("#e05bf1"),
                (int)SeatEnum.YELLOW => brg.UnityCommon.Utils.FromHex("#ffc562"),
                (int)SeatEnum.CYAN => brg.UnityCommon.Utils.FromHex("#46edff"),
                (int)SeatEnum.PURPLE => brg.UnityCommon.Utils.FromHex("#8354ee"),
                (int)SeatEnum.ORANGE => brg.UnityCommon.Utils.FromHex("#f18968"),
                (int)SeatEnum.TEAL => brg.UnityCommon.Utils.FromHex("#69d299"),
                (int)SeatEnum.BROWN => brg.UnityCommon.Utils.FromHex("#464ccc"),
                _ => Color.white
            };
        }
    }
}