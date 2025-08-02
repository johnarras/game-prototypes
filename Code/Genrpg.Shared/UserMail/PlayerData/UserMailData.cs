using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.UserMail.Constants;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Units.Loaders;
using Genrpg.Shared.Units.Mappers;
using MessagePack;
using Genrpg.Shared.DataStores.Interfaces;
using Genrpg.Shared.DataStores.Categories.PlayerData.ParentChild;

namespace Genrpg.Shared.UserMail.PlayerData
{
    [MessagePackObject]
    public class UserLetter : OwnerPlayerData
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string OwnerId { get; set; }
        [Key(2)] public long UserMailTypeId { get; set; }
    }


    [MessagePackObject]
    public class UserMailData : OwnerObjectList<UserLetter>, IServerOnlyData
    {
        [Key(0)] public override string Id { get; set; }
    }
    public class UserMailDto : OwnerDtoList<UserMailData, UserLetter> { }
    public class CrafterDataLoader : OwnerDataLoader<UserMailData, UserLetter> { }


    public class CrafterDataMapper : OwnerDataMapper<UserMailData, UserLetter, UserMailDto> { }
}
