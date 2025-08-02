using MessagePack;
using Genrpg.Shared.Website.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.BoardGame.Upgrades.WebApi
{
    [MessagePackObject]
    public class UpgradeBoardRequest : IClientUserRequest
    {
        [Key(0)] public long TileTypeId { get; set; }
    }
}
