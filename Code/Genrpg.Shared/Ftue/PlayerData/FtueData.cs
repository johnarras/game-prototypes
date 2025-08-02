using MessagePack;
using Genrpg.Shared.Utils.Data;
using Genrpg.Shared.Units.Loaders;
using Genrpg.Shared.Units.Mappers;
using Genrpg.Shared.DataStores.Categories.PlayerData.NoChild;
using Genrpg.Shared.Purchasing.PlayerData;

namespace Genrpg.Shared.Ftue.PlayerData
{
    /// <summary>
    /// Used to contain a list of currencies on objects that need it (like user and character)
    /// </summary>

    [MessagePackObject]
    public class FtueData : NoChildPlayerData
    {
        [Key(0)] public override string Id { get; set; }

        [Key(1)] public SmallIndexBitList CompletedFtues { get; set; } = new SmallIndexBitList();

        [Key(2)] public long CurrentFtueStepId { get; set; }

        public bool HaveCompletedFtue(long ftueId)
        {
            return CompletedFtues.HasBit(ftueId);
        }

        public void SetFtueCompleted(long ftueId)
        {
            CompletedFtues.SetBit(ftueId);
        }
    }
    public class FtueDataLoader : UnitDataLoader<FtueData> { }

    public class FtueDto : NoChildPlayerDataDto<FtueData> { }


    public class FtueDataMapper : NoChildUnitDataMapper<FtueData, FtueDto> { }
}
