using OxDb.SharedGame.MapMessages.Interfaces;
using OxDb.SharedGame.MapObjects.Entities;
using System.Collections.Generic;

namespace OxDb.MapServer.MapMessaging.Filters
{
    public abstract class ObjectFilter
    {
        public abstract List<MapObject> Filter(IMapApiMessage message, List<MapObject> initialTargets);
    }
}


