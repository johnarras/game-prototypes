using System.Collections.Generic;
using System.Linq;

namespace OxDb.SharedCore.Utils
{
    public static class RandUtils
    {
        public static T GetRandomElement<T>(IEnumerable<T> list, IRandom rand) where T : IWeightedItem
        {
            double chanceSum = list.Sum(x => x.Weight);

            double chanceChosen = rand.NextDouble() * chanceSum;

            foreach (T t in list)
            {
                chanceChosen -= t.Weight;
                if (chanceChosen <= 0)
                {
                    return t;
                }
            }
            return default(T);
        }

        public static T GetRandomEnchant<T>(IEnumerable<T> list, IRandom rand) where T : IItemEnchantWeight
        {
            double chanceSum = list.Sum(x => x.ItemEnchantWeight);

            double chanceChosen = rand.NextDouble() * chanceSum;

            foreach (T t in list)
            {
                chanceChosen -= t.ItemEnchantWeight;
                if (chanceChosen <= 0)
                {
                    return t;
                }
            }
            return default(T);
        }


        public static float SeedFloatRange(long seed, int mult, float minval, float maxval, int steps = 101)
        {
            if (steps < 1 || minval >= maxval)
            {
                return minval;
            }

            return minval + (maxval - minval) * (seed * mult % steps) / (1.0f * steps);
        }


        public static float FloatRange(double minVal, double maxVal, IRandom rand)
        {
            if (rand == null)
            {
                return (float)(minVal + maxVal / 2);
            }

            return (float)(minVal + rand.NextDouble() * (maxVal - minVal));
        }

        /// <summary>
        /// Multiply by a random number in range (1 + (-delta,delta))
        /// </summary>
        /// <param name="delta"></param>
        /// <param name="rand"></param>
        /// <returns></returns>
        public static float DeltaScale(double delta, IRandom rand)
        {
            return 1 + DeltaRange(delta, rand);
        }

        public static float DeltaRange(double delta, IRandom rand)
        {
            return FloatRange(-delta, delta, rand);
        }

        /// <summary>
        /// Pick a random range that generally goes from midVal-scaleDelta to midVal+scaleDelta, but
        /// give a certain number of chances (scaleTimes) to roll a number less than (scaleChance) to 
        /// increase the size of the random range by scaleDelta again.
        /// </summary>
        /// <param name="midval"></param>
        /// <param name="rand"></param>
        /// <param name="scaleTimes"></param>
        /// <param name="scaleChance"></param>
        /// <param name="scaleDelta"></param>
        /// <returns></returns>
        public static float ScaledRange(float midval, IRandom rand, int scaleTimes, double scaleChance)
        {
            if (rand == null)
            {
                return midval;
            }

            int totalScaleTimes = 0;
            for (int i = 0; i < scaleTimes; i++)
            {
                if (rand.NextDouble() < scaleChance)
                {
                    totalScaleTimes++;
                }
                else
                {
                    break;
                }
            }

            if (rand.NextDouble() < 0.5f)
            {
                return FloatRange(0.5f / (1 + totalScaleTimes), 1.0f, rand) * midval;
            }
            else
            {
                return FloatRange(1.0f, totalScaleTimes + 2, rand) * midval;
            }
        }


        public static int IntRange(int minVal, int maxVal, IRandom rand)
        {
            if (rand == null || minVal >= maxVal)
            {
                return (minVal + maxVal) / 2;
            }

            return minVal + rand.Next() % (maxVal - minVal + 1);
        }
        public static long LongRange(long minVal, long maxVal, IRandom rand)
        {
            if (rand == null || minVal >= maxVal)
            {
                return (minVal + maxVal) / 2;
            }

            return minVal + rand.NextLong() % (maxVal - minVal + 1);
        }

    }
}


