using OxDb.SharedCore.Utils;

namespace OxDb.SharedGame.Zones.WorldData
{
    public class ZoneUnitStatus : IWeightedItem
    {
        public long UnitTypeId { get; set; }

        /// <summary>
        /// Current population
        /// </summary>
        public double Weight { get; set; }


        /// <summary>
        /// How many have been killed since last update
        /// </summary>
        public int Killed { get; set; }



        public string Prefix { get; set; }


    }
}


