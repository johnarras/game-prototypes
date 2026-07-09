using OxDb.SharedCore.Utils.Data;

namespace OxDb.SharedGame.Units.Entities
{
    public class UnitGenData
    {
        public long UnitTypeId { get; set; }
        public long Level { get; set; }
        public long FactionTypeId { get; set; }
        public long ZoneId { get; set; }


        // Immediately create a unit and use it to store UnitTypeId, Id and Level.
        public Unit Unit { get; set; }
        public Point3F Pos { get; set; }
        public short Rot { get; set; }
        public object Parent { get; set; }
        public int StatPct { get; set; }
        public bool AllowNoParent { get; set; }

        public object ArtInstance;

        public UnitGenData()
        {
            StatPct = 100;
        }

    }
}


