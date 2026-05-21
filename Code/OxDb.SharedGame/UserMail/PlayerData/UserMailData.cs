using MessagePack;
using OxDb.SharedCore.DataStores.Interfaces;
using OxDb.SharedGame.DataStores.Categories.PlayerData.ParentChild;
using OxDb.SharedGame.Units.Loaders;
using OxDb.SharedGame.Units.Mappers;
using System.Collections.Generic;

namespace OxDb.SharedGame.UserMail.PlayerData
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


