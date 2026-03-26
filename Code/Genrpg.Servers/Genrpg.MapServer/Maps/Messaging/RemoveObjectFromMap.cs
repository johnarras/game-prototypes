using Genrpg.Shared.MapMessages;

namespace Genrpg.MapServer.Maps.Messaging
{
    public sealed class RemoveObjectFromMap : BaseMapMessage
    {
        public string ObjectId;
    }
}

