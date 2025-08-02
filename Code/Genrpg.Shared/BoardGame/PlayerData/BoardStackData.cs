using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.Units.Loaders;
using Genrpg.Shared.Units.Mappers;
using Genrpg.Shared.DataStores.Interfaces;
using Genrpg.Shared.DataStores.Categories.PlayerData.NoChild;
using Genrpg.Shared.DataStores.Categories.PlayerData.Users;
using Genrpg.Shared.Purchasing.PlayerData;

namespace Genrpg.Shared.BoardGame.PlayerData
{
    [MessagePackObject]
    public class BoardStackData : NoChildPlayerData, IUserData, IServerOnlyData
    {
        [Key(0)] public override string Id { get; set; }

        [Key(1)] public List<BoardData> Boards { get; set; } = new List<BoardData>();
    }


    public class BoardStackLoader : UnitDataLoader<BoardStackData> { }

    public class BoardStackDto : NoChildPlayerDataDto<BoardStackData> { }


    public class BoardStackDataMapper : NoChildUnitDataMapper<BoardStackData, BoardStackDto> { }
}
