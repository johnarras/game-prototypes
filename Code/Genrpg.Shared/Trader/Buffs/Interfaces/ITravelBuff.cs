namespace Genrpg.Shared.Trader.Buffs.Interfaces
{
    public interface ITravelBuff
    {
        int BonusSpeed { get; set; }
        int Capacity { get; set; }
        int RationsCost { get; set; }
        double ForageChance { get; set; }
        double GoodEventChance { get; set; }
        double BadEventChance { get; set; }
    }
}
