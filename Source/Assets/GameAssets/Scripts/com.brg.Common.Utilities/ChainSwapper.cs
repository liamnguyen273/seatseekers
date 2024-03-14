using System;
using System.Linq;

namespace com.brg.Common.Utils
{
    public class ChainSwapper<T>
    {
        private T[] _array;
    
        public ChainSwapper(int capacity)
        {
            _array = Enumerable.Repeat(default(T), capacity).ToArray();
        }

        public void Perform(int[] permutation, Func<int, T> accessor)
        {
            for (int i = 0; i < _array.Length; ++i)
            {
                _array[i] = accessor.Invoke(permutation[i]);
            }
        }

        public void Apply(Action<int, T> applier)
        {
            for (int i = 0; i < _array.Length; ++i)
            {
                applier.Invoke(i, _array[i]);
            }
        }
    }
}