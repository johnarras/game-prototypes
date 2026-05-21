namespace OxDb.SharedGame.Zones.Entities
{
    public class ZoneTypeOverride
    {
        /// <summary>
        /// Zone type to transition to
        /// </summary>
        public long ZoneTypeId { get; set; }
        /// <summary>
        /// The reason for the override, cold, hot, wet, dry, radiation...
        /// </summary>
        public int Reason { get; set; }
        public string Name { get; set; }
    }
}


