using MessagePack;
using Genrpg.Shared.DataStores.Categories.PlayerData.NoChild;
using Genrpg.Shared.DataStores.Categories.PlayerData.Users;

namespace Genrpg.Shared.Trader.Caravans.PlayerData
{
    [MessagePackObject]
    public class CaravanData : NoChildPlayerData, IUserData
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public double Speed { get; set; }
        [Key(2)] public long TotalWeight { get; set; }
        [Key(3)] public long Food { get; set; }
        [Key(4)] public long Water { get; set; }
    }
}
