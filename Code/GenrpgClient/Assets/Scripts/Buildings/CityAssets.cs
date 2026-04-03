using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Buildings
{
    public class CityAssets : BaseBehaviour
    {
        public List<WeightedCrawlerBuilding> Buildings;

        public bool IsReady()
        {
            if (Buildings == null || Buildings.Count == 0)
            {
                return false;
            }

            if (Buildings.Any(x => !x.IsReady()))
            {
                return false;
            }

            return true;
        }
    }
}


