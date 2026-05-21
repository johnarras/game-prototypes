using OxDb.SharedCore.Client.Interfaces;

namespace Assets.Scripts.Crawler.Maps.ClientEvents
{
    public class RedrawMapCell : IClientEvent
    {
        public int X { get; set; }
        public int Z { get; set; }
        public object Data { get; set; }
    }
}


