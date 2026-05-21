using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.MapObjects.Entities;
using System.Collections.Generic;

namespace OxDb.MapServer.Maps.Filters
{
    public interface IObjectFilter : ISetupDictionaryItem<long>
    {
        List<MapObject> Filter(MapObject obj, List<MapObject> initialTargets);
    }
}


