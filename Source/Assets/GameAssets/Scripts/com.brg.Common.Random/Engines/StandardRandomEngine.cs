namespace com.brg.Common.Random.Engines
{
    internal class StandardRandomEngine : IRandomEngine
    {
        private int _seed;
        private long _genCount;
        private System.Random _random;

        private StandardRandomEngine(int seed, long genCount) : this(seed)
        {
            while (_genCount < genCount)
            {
                GetInteger();
            }
        }
        
        public StandardRandomEngine(int seed)
        {
            _seed = seed;
            _genCount = 0;
            Reset();
        }

        public int GetSeed()
        {
            return _seed;
        }

        public void Reseed(int seed)
        {
            _seed = seed;
            Reset();
        }

        public IRandomEngine Clone()
        {
            return new StandardRandomEngine(_seed, _genCount);
        }


        public float GetFloat()
        {
            return (float)_random.NextDouble();
        }

        public float GetFloat(float max)
        {
            return GetFloat() * max;
        }

        public float GetFloat(float min, float max)
        {
            return GetFloat() * (max - min) + min;
        }

        public int GetInteger()
        {
            _genCount++;
            return _random.Next();
        }

        public int GetInteger(int maxExclusive)
        {
            _genCount++;
            return _random.Next(maxExclusive);
        }

        public int GetInteger(int minInclusive, int maxExclusive)
        {
            _genCount++;
            return _random.Next(minInclusive, maxExclusive);
        }

        public void Reset()
        {
            _random = new System.Random(_seed);
            _genCount = 0;
        }
    }
}
