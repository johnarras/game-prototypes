namespace Genrpg.Shared.Trader.Caravans.Entities
{
    public class CaravanStatusBlock
    {
        public long DiceSpeed { get; set; }
        public long ItemCount { get; set; }

        // Add bonuses for movement stats here.
        public long BonusSpeed { get; set; }
        public long RationsCost { get; set; }
        public long Capacity { get; set; }

        // Add chances for certain events here
        public double ForageChance { get; set; }
        public double BadEventChance { get; set; }
        public double GoodEventChance { get; set; }

    }
}
