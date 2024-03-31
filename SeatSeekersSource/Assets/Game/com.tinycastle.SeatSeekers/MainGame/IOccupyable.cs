namespace com.tinycastle.SeatSeekers
{
    public interface IOccupyable
    {
        public SeatData Data { get; }
        public bool Occupy(int x, int y);
        public bool CanEnterFrom(int fx, int fy);
        public void SetData(SeatData data);
    }
}