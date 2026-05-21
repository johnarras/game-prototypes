using OxDb.SharedGame.MapMessages;
using OxDb.SharedGame.MapObjects.Entities;

namespace OxDb.MapServer.Maps.Messaging
{
    public class RemoveObjectFromGridCell : BaseMapMessage
    {
        public MapObjectGridData GridData { get; set; }
        public MapObjectGridItem GridItem { get; set; }
    }
}


