namespace com.brg.Common.Random
{
    public interface IRandomEngine
    {
        public int GetSeed();
        public void Reseed(int seed);
        public IRandomEngine Clone();

        public void Reset();
        public int GetInteger();
        public int GetInteger(int maxExclusive);
        public int GetInteger(int minInclusive, int maxExclusive);
        public float GetFloat();
        public float GetFloat(float max);
        public float GetFloat(float min, float max);
    }
}