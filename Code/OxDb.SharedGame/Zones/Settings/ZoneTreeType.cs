namespace OxDb.SharedGame.Zones.Settings
{
    /// <summary>
    /// Used to override data about trees in the zone and zone type
    /// </summary>
    public class ZoneTreeType
    {
        public long TreeTypeId { get; set; }
        public float PopulationScale { get; set; }
        public string Name { get; set; }

        public ZoneTreeType()
        {
            PopulationScale = 1.0f;
        }
    }
}


