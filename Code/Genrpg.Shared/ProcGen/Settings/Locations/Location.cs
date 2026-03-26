using System.Collections.Generic;

namespace Genrpg.Shared.ProcGen.Settings.Locations
{
    /// <summary>
    /// This contains information about this location.
    /// The faction information is separated, I guess
    /// there might be a chance for multiple factions
    /// to be here or something. Not sure, but
    /// it should be separate
    /// 
    /// </summary>
    public class Location
    {
        /// <summary>
        /// Location id in the zone
        /// </summary>
        public string Id { get; set; }

        public long ZoneId { get; set; }
        /// <summary>
        ///  What kind of location this is
        /// </summary>
        public long LocationTypeId { get; set; }
        /// <summary>
        /// Name of the location
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Description of the location
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Location xpos on map
        /// </summary>
        public int CenterX { get; set; }
        /// <summary>
        /// Location ypos on map
        /// </summary>
        public int CenterZ { get; set; }

        /// <summary>
        /// MyRandom seed for generating content
        /// </summary>
        public long Seed { get; set; }

        /// <summary>
        /// XSize in units
        /// </summary>
        public int XSize { get; set; }
        /// <summary>
        /// YSize in units
        /// </summary>
        public int ZSize { get; set; }

        public string ExtraZone { get; set; }

        public List<LocationPlace> Places { get; set; } = new List<LocationPlace>();

        public bool IsRectangular()
        {
            return false;
        }

        public void CleanForClient()
        {
            Places = new List<LocationPlace>();
        }

    }
}


