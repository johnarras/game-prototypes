using Genrpg.Shared.Interfaces;
using Genrpg.Shared.MapObjects.Entities;
using System.Collections.Generic;

namespace Genrpg.MapServer.Maps.Filters
{
    public interface IObjectFilter : ISetupDictionaryItem<long>
    {
        List<MapObject> Filter(MapObject obj, List<MapObject> initialTargets);
    }
}


