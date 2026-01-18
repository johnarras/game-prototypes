namespace Genrpg.Shared.Trader.Buffs.Interfaces
{
    public interface ITravelBuff
    {
        long BonusSpeed { get; set; }
        long Capacity { get; set; }
        long RationsCost { get; set; }
        double ForageChance { get; set; }
        double GoodEventChance { get; set; }
        double BadEventChance { get; set; }
    }
}
