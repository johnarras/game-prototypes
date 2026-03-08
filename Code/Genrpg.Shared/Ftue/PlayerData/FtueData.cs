using Genrpg.Shared.DataStores.Categories.PlayerData.NoChild;
using Genrpg.Shared.DataStores.Categories.PlayerData.Users;
using Genrpg.Shared.Units.Loaders;
using Genrpg.Shared.Units.Mappers;
using Genrpg.Shared.Utils.Data;
using MessagePack;

namespace Genrpg.Shared.Ftue.PlayerData
{
    /// <summary>
    /// Used to contain a list of currencies on objects that need it (like user and character)
    /// </summary>

    [MessagePackObject]
    public class FtueData : UniquePersonalUserData, IUserData
    {
        [Key(0)] public override string Id { get; set; }

        [Key(1)] public SmallIndexBitList CompletedFtues { get; set; } = new SmallIndexBitList();

        [Key(2)] public long CurrentFtueStepId { get; set; }

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


