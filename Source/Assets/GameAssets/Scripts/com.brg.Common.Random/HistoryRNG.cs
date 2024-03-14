using System;
using System.Collections.Generic;
using System.Linq;
using com.brg.Common.Utils;

namespace com.brg.Common.Random
{
    public class HistoryRNG : IRandomEngine
    {
        private List<int> _history = new ();
        private int _currentIndex = 0;
        private readonly IRandomEngine _rng;

        public HistoryRNG(IRandomEngine rng)
        {
            _rng = rng;
        }

        public int GetSeed()
        {
            return _rng.GetSeed();
        }

        public void Reseed(int seed)
        {
            _rng.Reseed(seed);
            _history.Clear();
            _currentIndex = 0;
        }

        public IRandomEngine Clone()
        {
            var clone = new HistoryRNG(_rng.Clone())
            {
                _history = _history.ToList(),
                _currentIndex = _currentIndex
            };
            return clone;
        }

        public void Reset()
        {
            _rng.Reset();
            _history.Clear();
            _currentIndex = 0;
        }

        public int GetInteger()
        {
            while (_currentIndex >= _history.Count)
            {
                var integer = _rng.GetInteger();
                _history.Add(integer);
            }

            return _history[_currentIndex];
        }

        public void Rewind(int steps)
        {
            _currentIndex = Math.Max(0, _currentIndex - steps);
        }

        public void RewindTo(int index)
        {
            _currentIndex = Utilities.Clamp(index, 0, _history.Count - 1);
        }

        public int GetInteger(int maxExclusive)
        {
            return GetInteger(0, maxExclusive);
        }

        public int GetInteger(int minInclusive, int maxExclusive)
        {
            var r = GetInteger();
            var range = maxExclusive - minInclusive;
            int i = r % range;
            return minInclusive + i;
        }

        public float GetFloat()
        {
            // Not the best, but this will do
            var r = Math.Abs(GetInteger());
            var l = r.CountDigits();
            return r / MathF.Pow(10, l);
        }

        public float GetFloat(float max)
        {
            return GetFloat() * max;
        }

        public float GetFloat(float min, float max)
        {
            return GetFloat() * (max - min) + min;
        }
    }
}