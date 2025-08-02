using MessagePack;
using Genrpg.Shared.BoardGame.Entities;
using Genrpg.Shared.Website.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.BoardGame.Upgrades.WebApi
{
    [MessagePackObject]
    public class UpgradeBoardResponse : IWebResponse
    {
        [Key(0)] public bool Success { get; set; }
        [Key(1)] public string Message { get; set; }
        [Key(2)] public long TileTypeId { get; set; }
        [Key(3)] public long NewTier { get; set; }

        [Key(4)] public UpgradeCosts Costs { get; set; }
    }
}
