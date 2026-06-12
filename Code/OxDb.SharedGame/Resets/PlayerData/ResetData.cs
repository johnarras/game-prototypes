using MessagePack;
using OxDb.SharedGame.Core.PlayerData;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Constants;
using OxDb.SharedGame.DataStores.Categories.PlayerData.NoChild;
using OxDb.SharedGame.Units.Mappers;
using System;
namespace OxDb.SharedGame.Resets.PlayerData
{
    /// <summary>
    /// Used to contain a list of currencies on objects that need it (like user and character)
    /// </summary>

    [MessagePackObject]
    public class ResetData : UniquePersonalUserData
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public int ConsecutiveResetDays { get; set; }
        [Key(2)] public DateTime LastResetDay { get; set; } = DateTime.UtcNow.Date.AddDays(-1);

        public override PersonalDataAccumulation GetAccumulation()
        {
            return new PersonalDataAccumulation();
        }

        public override int GetOffsetBit()
        {
            return EPersonalDataOffsetBits.Resets;
        }
    }

    [MessagePackObject]
    public class ResetDto : NoChildPlayerDataDto<ResetData>
    {
        [Key(0)] public override ResetData Parent { get; set; }
        [Key(1)] public override string Id { get; set; }
    }


    public class ResetDataMapper : NoChildUnitDataMapper<ResetData, ResetDto> { }
}


