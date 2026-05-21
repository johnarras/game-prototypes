using OxDb.SharedGame.MapMessages.Interfaces;
using OxDb.SharedGame.MapObjects.Entities;
using System.Collections.Generic;
using System.Linq;

namespace OxDb.MapServer.MapMessaging.Filters
{
    public class ToPlayerFilter : ObjectFilter
    {
        public override List<MapObject> Filter(IMapApiMessage message, List<MapObject> initialTargets)
        {
            return new List<MapObject>(initialTargets.Where(x => x.IsUnit()));
        }
    }
}


