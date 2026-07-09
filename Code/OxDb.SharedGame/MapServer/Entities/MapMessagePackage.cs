using OxDb.SharedCore.Utils;
using OxDb.SharedGame.MapMessages.Interfaces;
using OxDb.SharedGame.MapObjects.Entities;

namespace OxDb.SharedGame.MapServer.Entities
{
    public class MapMessagePackage
    {
        public MapObject MapObject { get; set; }
        public IMapMessage Message { get; set; }
        public IMapMessageHandler Handler { get; set; }
        public float delaySeconds { get; set; } = 0;

        public void Clear()
        {
            MapObject = null;
            Message = null;
            Handler = null;
            delaySeconds = 0;
        }
    }
}


