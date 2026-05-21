namespace OxDb.SharedGame.Trader.Caravans.Entities
{
    public class CaravanStatusBlock
    {
        public int DiceSpeed { get; set; }
        public int ItemCount { get; set; }

        // Add bonuses for movement stats here.
        public int BonusSpeed { get; set; }
        public int RationsCost { get; set; }
        public int Capacity { get; set; }

        // Add chances for certain events here
        public double ForageChance { get; set; }
        public double Luck { get; set; }
        public double GoodEventChance { get; set; }

    }
}
