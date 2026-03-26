using Genrpg.Shared.MapMessages.Interfaces;
using Genrpg.Shared.MapObjects.Entities;
using System.Collections.Generic;

namespace Genrpg.MapServer.MapMessaging.Filters
{
    public abstract class ObjectFilter
    {
        public abstract List<MapObject> Filter(IMapApiMessage message, List<MapObject> initialTargets);
    }
}


