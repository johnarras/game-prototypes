using MessagePack;
using Genrpg.Shared.Accounts.PlayerData;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.BoardGame.Entities
{
    [MessagePackObject]
    public class UpgradeReagent
    {
        [Key(0)] public long UserCoinTypeId { get; set; }
        [Key(1)] public long CurrQuantity { get; set; }
        [Key(2)] public long RequiredQuantity { get; set; }
        [Key(3)] public long MissingQuantity { get; set; }
    }


    [MessagePackObject]
    public class UpgradeCosts
    {
        [Key(0)] public bool CanUpgradeNow { get; set; }
        [Key(1)] public long TileTypeId { get; set; }
        [Key(2)] public long CurrUpgradeTier { get; set; }
        [Key(3)] public long MaxUpgradeTier { get; set; }
        [Key(4)] public long NextUpgradeTier { get; set; }
        [Key(5)] public long ExtraTokenCost { get; set; }
        [Key(6)] public string ErrorMessage { get; set; }
        [Key(7)] public List<UpgradeReagent> Reagents { get; set; } = new List<UpgradeReagent>();

    }
}
