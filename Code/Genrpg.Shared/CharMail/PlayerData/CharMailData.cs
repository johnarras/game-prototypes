using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.CharMail.Constants;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Units.Loaders;
using Genrpg.Shared.Units.Mappers;
using MessagePack;
using Genrpg.Shared.DataStores.Interfaces;
using Genrpg.Shared.DataStores.Categories.PlayerData.ParentChild;

namespace Genrpg.Shared.CharMail.PlayerData
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
    public class CharMailDto : OwnerDtoList<CharMailData, CharLetter> { }
    public class CrafterDataLoader : OwnerDataLoader<CharMailData, CharLetter> { }


    public class CrafterDataMapper : OwnerDataMapper<CharMailData, CharLetter, CharMailDto> { }
}
