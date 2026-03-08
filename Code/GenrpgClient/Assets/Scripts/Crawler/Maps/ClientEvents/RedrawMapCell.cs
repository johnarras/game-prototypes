using Genrpg.Shared.Client.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Crawler.Maps.ClientEvents
{
    public class RedrawMapCell : IClientEvent
    {
        public int X { get; set; }
        public int Z { get; set; }
        public object Data { get; set; }
    }
}


