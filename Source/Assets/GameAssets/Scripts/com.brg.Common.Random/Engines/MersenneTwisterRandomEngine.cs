using System;

namespace com.brg.Common.Random.Engines
{
    internal class MersenneTwisterRandomEngine : IRandomEngine
    {
        private int _seed;

        protected const int N = 624;
        protected const int M = 397;
        protected const uint MATRIX_A = 0x9908b0dfU;
        protected const uint UPPER_MASK = 0x80000000U;
        protected const uint LOWER_MASK = 0x7fffffffU;
        protected const uint TEMPER1 = 0x9d2c5680U;
        protected const uint TEMPER2 = 0xefc60000U;
        protected const int TEMPER3 = 11;
        protected const int TEMPER4 = 7;
        protected const int TEMPER5 = 15;
        protected const int TEMPER6 = 18;

        protected uint[] _mt;
        protected int _mti;
        private uint[] _mag01;

        public MersenneTwisterRandomEngine(int seed)
        {
            _mt = Array.Empty<uint>();
            _mag01 = Array.Empty<uint>();
            _seed = seed;
            Reset();
        }

        private MersenneTwisterRandomEngine(int seed, uint[] mt, int mti, uint[] mag01)
        {
            _seed = seed;
            _mt = mt;
            _mti = mti;
            _mag01 = mag01;
        }

        public void Reset()
        {
            _mt = new uint[N];
            _mti = N + 1;
            _mag01 = new uint[] { 0x0U, MATRIX_A };

            _mt[0] = (uint)_seed;
            for (var i = 1; i < N; i++)
                _mt[i] = (uint)(1812433253 * (_mt[i - 1] ^ _mt[i - 1] >> 30) + i);
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
            return new MersenneTwisterRandomEngine(_seed, (uint[])_mt.Clone(), _mti, (uint[])_mag01.Clone());
        }

        public float GetFloat()
        {
            uint r1, r2;
            r1 = NextUInt32();
            r2 = NextUInt32();
            return (r1 * (float)(2 << 8) + r2) / (2 << 24);
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
            var r = NextUInt32();
            return unchecked((int)r);
        }

        public int GetInteger(int maxExclusive)
        {
            return GetInteger(0, maxExclusive);
        }

        public int GetInteger(int minInclusive, int maxExclusive)
        {
            var r = NextUInt32();
            var range = maxExclusive - minInclusive;
            int i = (int)(r % range);
            return minInclusive + i;
        }

        private uint NextUInt32()
        {
            uint y;
            if (_mti >= N) { Generate(); _mti = 0; }
            y = _mt[_mti++];
            y ^= y >> TEMPER3;
            y ^= y << TEMPER4 & TEMPER1;
            y ^= y << TEMPER5 & TEMPER2;
            y ^= y >> TEMPER6;
            return y;
        }

        private void Generate()
        {
            int kk = 1;
            uint y;
            uint p;

            y = _mt[0] & UPPER_MASK;
            do
            {
                p = _mt[kk];
                _mt[kk - 1] = _mt[kk + (M - 1)] ^ (y | p & LOWER_MASK) >> 1 ^ _mag01[p & 1];
                y = p & UPPER_MASK;
            } while (++kk < N - M + 1);
            do
            {
                p = _mt[kk];
                _mt[kk - 1] = _mt[kk + (M - N - 1)] ^ (y | p & LOWER_MASK) >> 1 ^ _mag01[p & 1];
                y = p & UPPER_MASK;
            } while (++kk < N);
            p = _mt[0];
            _mt[N - 1] = _mt[M - 1] ^ (y | p & LOWER_MASK) >> 1 ^ _mag01[p & 1];
        }
    }
}
