using OxDb.SharedGame.MapMessages;

namespace OxDb.MapServer.Maps.Messaging
{
    public sealed class RemoveObjectFromMap : BaseMapMessage
    {
        public string ObjectId;
    }
}

