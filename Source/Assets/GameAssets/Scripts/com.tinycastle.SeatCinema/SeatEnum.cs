using com.brg.UnityCommon;
using UnityEngine;

namespace com.tinycastle.SeatCinema
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
                (int)SeatEnum.BLUE => Color.blue,
                (int)SeatEnum.GREEN => Color.green,
                (int)SeatEnum.RED => Color.red,
                (int)SeatEnum.MAGENTA => Color.magenta,
                (int)SeatEnum.YELLOW => Color.yellow,
                (int)SeatEnum.CYAN => Color.cyan,
                (int)SeatEnum.PURPLE => Utilities.FromHex("#6824f0"),
                (int)SeatEnum.ORANGE => Utilities.FromHex("#fc8b19"),
                (int)SeatEnum.TEAL => Utilities.FromHex("#19fcc0"),
                (int)SeatEnum.BROWN => Utilities.FromHex("#54473e"),
                _ => Color.white
            };
        }
    }
}