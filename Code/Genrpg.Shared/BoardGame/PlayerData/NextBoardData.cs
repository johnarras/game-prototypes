using MessagePack;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using Genrpg.Shared.Website.Interfaces;

namespace Genrpg.Shared.BoardGame.PlayerData
{
    [MessagePackObject]
    public class NextBoardData : IUnitData, IWebResponse
    {
        [Key(0)] public string Id { get; set; }
        [Key(1)] public BoardData NextBoard { get; set; }

        public virtual IUnitData Unpack() { return this; }
    }
}
