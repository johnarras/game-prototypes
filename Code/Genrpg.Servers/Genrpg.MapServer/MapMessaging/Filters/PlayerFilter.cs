using Genrpg.Shared.MapMessages.Interfaces;
using Genrpg.Shared.MapObjects.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Genrpg.MapServer.MapMessaging.Filters
{
    public class PlayerFilter : ObjectFilter
    {
        public override List<MapObject> Filter(IMapApiMessage message, List<MapObject> initialTargets)
        {
            return new List<MapObject>(initialTargets.Where(x => x.IsPlayer()));
        }
    }
}


