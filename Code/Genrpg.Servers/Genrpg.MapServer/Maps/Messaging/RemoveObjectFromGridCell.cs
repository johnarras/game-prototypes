using Genrpg.Shared.MapMessages;
using Genrpg.Shared.MapObjects.Entities;

namespace Genrpg.MapServer.Maps.Messaging
{
    public class RemoveObjectFromGridCell : BaseMapMessage
    {
        public MapObjectGridData GridData { get; set; }
        public MapObjectGridItem GridItem { get; set; }
    }
}


