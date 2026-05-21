using MessagePack;
using OxDb.SharedCore.DataStores.Interfaces;
using OxDb.SharedGame.DataStores.Categories.PlayerData.ParentChild;
using OxDb.SharedGame.Units.Loaders;
using OxDb.SharedGame.Units.Mappers;
using System.Collections.Generic;

namespace OxDb.SharedGame.CharMail.PlayerData
{
    [MessagePackObject]
    public class CharLetter : OwnerPlayerData
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public override string OwnerId { get; set; }
        [Key(2)] public long CharLetterTypeId { get; set; }
    }


    [MessagePackObject]
    public class CharMailData : OwnerObjectList<CharLetter>, IServerOnlyData
    {
        [Key(0)] public override string Id { get; set; }
    }
    [MessagePackObject]
    public class CharMailDto : OwnerDtoList<CharMailData, CharLetter>
    {
        [Key(0)] public override List<CharLetter> Children { get; set; }
        [Key(1)] public override CharMailData Parent { get; set; }
        [Key(2)] public override string Id { get; set; }
    }
    public class CrafterDataLoader : OwnerDataLoader<CharMailData, CharLetter> { }


    public class CrafterDataMapper : OwnerDataMapper<CharMailData, CharLetter, CharMailDto> { }
}


