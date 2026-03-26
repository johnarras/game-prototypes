using Genrpg.Shared.Utils.Data;

namespace Genrpg.Shared.Trader.Caravans.Entities
{
    public class CaravanTravelInfo
    {
        public long Days { get; set; }
        public SmallIdLongCollection CurrenciesPerDay { get; set; } = new SmallIdLongCollection();
        public int DiceSpeed { get; set; }
        public int BonusSpeed { get; set; }
        public long MaxSize { get; set; }
        public long SizeUsed { get; set; }
        public long MaxInventory { get; set; }
        public long InventoryUsed { get; set; }
    }
}


