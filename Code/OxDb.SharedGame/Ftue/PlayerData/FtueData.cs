using MessagePack;
using OxDb.SharedCore.Utils.Data;
using OxDb.SharedGame.Core.PlayerData;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Constants;
using OxDb.SharedGame.DataStores.Categories.PlayerData.NoChild;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Users;
using OxDb.SharedGame.Units.Loaders;
using OxDb.SharedGame.Units.Mappers;
using System.ComponentModel.Design;
using System.Reflection;

namespace OxDb.SharedGame.Ftue.PlayerData
{
    /// <summary>
    /// Used to contain a list of currencies on objects that need it (like user and character)
    /// </summary>

    [MessagePackObject]
    public class FtueData : UniquePersonalUserData, IUserData
    {
        public override int GetOffsetBit() { return EPersonalDataOffsetBits.Ftue; }


        public override PersonalDataAccumulation GetAccumulation()
        {
            PersonalDataAccumulation accumulation = new PersonalDataAccumulation()
            {
            };

            accumulation.SumValues.Add(CompletedFtues.GetBitCount());

            return accumulation;
        }

        [Key(0)] public override string Id { get; set; }

        [Key(1)] public SmallIndexBitList CompletedFtues { get; set; } = new SmallIndexBitList();

        [Key(2)] public long CurrentFtueStepId { get; set; }

        [Key(3)] public long PrevFtueStepId { get; set; }

        public bool HaveCompletedFtue(long ftueId)
        {
            return CompletedFtues.HasBitIndex(ftueId);
        }

        public void SetFtueCompleted(long ftueId)
        {
            CompletedFtues.SetBitIndex(ftueId);
        }
    }
    public class FtueDataLoader : UnitDataLoader<FtueData> { }

    [MessagePackObject]
    public class FtueDto : NoChildPlayerDataDto<FtueData>
    {
        [Key(0)] public override FtueData Parent { get; set; }
        [Key(1)] public override string Id { get; set; }
    }


    public class FtueDataMapper : NoChildUnitDataMapper<FtueData, FtueDto> { }
}


