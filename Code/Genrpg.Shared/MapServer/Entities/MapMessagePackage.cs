using Genrpg.Shared.Errors.Messages;
using Genrpg.Shared.MapMessages.Interfaces;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Utils;
using System.Threading.Tasks;

namespace Genrpg.Shared.MapServer.Entities
{
    public class MapMessagePackage
    {
        public MapObject mapObject;
        public IMapMessage message;
        public IMapMessageHandler handler;
        public float delaySeconds = 0;

        public async Task Process(IRandom rand)
        {
            await handler.Process(rand, this);
        }

        public void SendError(MapObject obj, string errorText)
        {
            obj.AddMessage(new ErrorMessage(errorText));
        }

        public void Clear()
        {
            mapObject = null;
            message = null;
            handler = null;
        }
    }
}


