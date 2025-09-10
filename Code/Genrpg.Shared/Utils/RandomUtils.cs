using System.Collections.Generic;
using System.Linq;

namespace Genrpg.Shared.Utils
{
    public static class RandomUtils
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
    }
}
