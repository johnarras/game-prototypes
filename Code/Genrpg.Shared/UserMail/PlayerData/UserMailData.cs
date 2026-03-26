using Genrpg.Shared.DataStores.Categories.PlayerData.ParentChild;
using Genrpg.Shared.DataStores.Interfaces;
using Genrpg.Shared.Units.Loaders;
using Genrpg.Shared.Units.Mappers;
using MessagePack;
using System.Collections.Generic;

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
    [MessagePackObject]
    public class UserMailDto : OwnerDtoList<UserMailData, UserLetter>
    {
        [Key(0)] public override List<UserLetter> Children { get; set; }
        [Key(1)] public override UserMailData Parent { get; set; }
        [Key(2)] public override string Id { get; set; }
    }
    public class CrafterDataLoader : OwnerDataLoader<UserMailData, UserLetter> { }


    public class CrafterDataMapper : OwnerDataMapper<UserMailData, UserLetter, UserMailDto> { }
}


