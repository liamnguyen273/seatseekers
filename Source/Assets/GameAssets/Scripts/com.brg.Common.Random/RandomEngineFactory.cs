using System;
using com.brg.Common.Random.Engines;

namespace com.brg.Common.Random
{
    public static class RandomEngineFactory
    {
        public static IRandomEngine CreateEngine(RandomEngineEnum engine, int seed, int extraParams = -1)
        {
            return engine switch
            {
                RandomEngineEnum.STANDARD => new StandardRandomEngine(seed),
                RandomEngineEnum.MERSENNE_TWISTER => new MersenneTwisterRandomEngine(seed),
                RandomEngineEnum.SFMT => new SFMTRandomEngine(seed, extraParams),
                _ => throw new UnreachableException()
            };
        }

        public static T Clone<T>(T other) where T : IRandomEngine
        {
            return (T)other.Clone();
        }
    }

    public class UnreachableException : Exception
    {
        
    }
}