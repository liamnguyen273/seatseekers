using System.Collections.Generic;
using System.Linq;

namespace com.brg.Common.Random
{
    public class RandomEngineGroup<T>
    {
        private readonly Dictionary<T, IRandomEngine> _dictionary;

        public RandomEngineGroup()
        {
            _dictionary = new Dictionary<T, IRandomEngine>();
        }

        public IRandomEngine GetEngine(T identifier)
        {
            return _dictionary[identifier];
        }

        public int GetSeed(T identifier)
        {
            return _dictionary[identifier].GetSeed();
        }

        public void Reseed(T identifier, int seed)
        {
            _dictionary[identifier].Reseed(seed);
        }

        public void AddEngine(T identifier, IRandomEngine engine)
        {
            _dictionary.Add(identifier, engine);
        }

        public void RemoveEngine(T identifier)
        {
            _dictionary.Remove(identifier);
        }

        public void Reset(T identifier)
        {
            _dictionary[identifier].Reset();
        }

        public int GetInteger(T identifier)
        {
            return _dictionary[identifier].GetInteger();
        }

        public int GetInteger(T identifier, int maxEclusive)
        {
            return _dictionary[identifier].GetInteger(maxEclusive);
        }

        public int GetInteger(T identifier, int minInclusive, int maxExclusive)
        {
            return _dictionary[identifier].GetInteger(minInclusive, maxExclusive);
        }

        public float GetFloat(T identifier)
        {
            return _dictionary[identifier].GetFloat();
        }

        public float GetFloat(T identifier, float max)
        {
            return _dictionary[identifier].GetFloat(max);
        }

        public float GetFloat(T identifier, float min, float max)
        {
            return _dictionary[identifier].GetFloat(min, max);
        }

        public static RandomEngineGroup<T> Create(RandomEngineEnum type, IEnumerable<T> identifiers, IRandomEngine seedRng)
        {
            var group = new RandomEngineGroup<T>();

            foreach (var identifier in identifiers)
            {
                group.AddEngine(identifier, RandomEngineFactory.CreateEngine(type, seedRng.GetInteger()));
            }
            
            return group;
        }
        
        public static RandomEngineGroup<T> Create(RandomEngineEnum type, IEnumerable<T> identifiers, IEnumerable<int> seeds)
        {
            var group = new RandomEngineGroup<T>();

            foreach (var (identifier, seed) in identifiers.Zip(seeds, (f, s) => (f, s)))
            {
                group.AddEngine(identifier, RandomEngineFactory.CreateEngine(type, seed));
            }
            
            return group;
        }
    }
}