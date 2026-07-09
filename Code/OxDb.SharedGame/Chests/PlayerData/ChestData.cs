using MessagePack;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.DataStores.Categories.PlayerData.ParentChild;
using OxDb.SharedGame.Units.Loaders;
using OxDb.SharedGame.Units.Mappers;
using System;
using System.Collections.Generic;

namespace OxDb.SharedGame.Chests.PlayerData
{
    /// <summary>
    /// Used to contain a list of currencies on objects that need it (like user and character)
    /// </summary>

    [MessagePackObject]
    public class ChestData : OwnerObjectList<ChestStatus>
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string VersionTag { get; set; }

    }

    [MessagePackObject]
    public class ChestStatus : OwnerPlayerData, IId
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string OwnerId { get; set; }
        [Key(2)] public long IdKey { get; set; }
        [Key(3)] public int Slot { get; set; }
        [Key(4)] public DateTime UnlockTime { get; set; } = DateTime.MinValue;
        [Key(5)] public override string VersionTag { get; set; }

    }

    [MessagePackObject]
    public class ChestDto : OwnerDtoList<ChestData, ChestStatus>
    {
        [Key(0)] public override List<ChestStatus> Children { get; set; }
        [Key(1)] public override ChestData Parent { get; set; }
        [Key(2)] public override string Id { get; set; }
    }

    public class ChestDataLoader : OwnerIdDataLoader<ChestData, ChestStatus> { }


    public class ChestDataMapper : OwnerDataMapper<ChestData, ChestStatus, ChestDto> { }
}


