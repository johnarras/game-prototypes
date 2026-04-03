using Genrpg.Shared.DataStores.Categories.PlayerData.NoChild;
using Genrpg.Shared.Units.Mappers;
using Genrpg.Shared.Utils.Data;
using MessagePack;
using System;
namespace Genrpg.Shared.Resets.PlayerData
{
    /// <summary>
    /// Used to contain a list of currencies on objects that need it (like user and character)
    /// </summary>

    [MessagePackObject]
    public class ResetData : NoChildPlayerData
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public int ConsecutiveResetDays { get; set; }
        [Key(4)] public DateTime LastResetDay { get; set; } = DateTime.UtcNow.Date.AddDays(-1);

    }

    [MessagePackObject]
    public class ResetDto : NoChildPlayerDataDto<ResetData>
    {
        [Key(0)] public override ResetData Parent { get; set; }
        [Key(1)] public override string Id { get; set; }
    }


    public class ResetDataMapper : NoChildUnitDataMapper<ResetData, ResetDto> { }
}


