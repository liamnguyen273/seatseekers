namespace com.tinycastle.SeatSeekers
{
    public struct SeatData
    {
        public const int DOUBLE_SEAT_LEFT = 100;
        public const int DOUBLE_SEAT_RIGHT = 200;
        public const int MAX_COLOR = 10;
        
        public int X;
        public int Y;
        public int Value;

        public bool IsObstacle => CheckObstacle(Value);
        public bool IsDouble => CheckDouble(Value);
        public bool IsDoubleLeft => CheckDoubleLeft(Value);
        public bool IsDoubleRight => CheckDoubleRight(Value);
        public bool IsInvalid => X < 0 || Y < 0;
        public int Color => Value % DOUBLE_SEAT_LEFT;
        
        public static SeatData Invalid =>
            new()
            {
                X = -1,
                Y = -1,
                Value = (int)SeatEnum.NONE
            };

        public static bool CheckObstacle(int value)
        {
            return (value % DOUBLE_SEAT_LEFT) > (int)SeatEnum.BROWN;
        }
        
        public static bool CheckDouble(int value)
        {
            return value > DOUBLE_SEAT_LEFT;
        }
        
        public static bool CheckDoubleLeft(int value)
        {
            return value is > DOUBLE_SEAT_LEFT and < DOUBLE_SEAT_RIGHT;
        }
        
        public static bool CheckDoubleRight(int value)
        {
            return value > DOUBLE_SEAT_RIGHT;
        }

        public static bool CheckSameColor(int value1, int value2)
        {
            return value1 % DOUBLE_SEAT_LEFT == value2 % DOUBLE_SEAT_LEFT;
        }
    }
}