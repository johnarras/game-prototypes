using MessagePack;
using Genrpg.Shared.Website.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.PlayMultiplier.WebApi
{
    public class SetPlayMultResponse : IWebResponse
    {
        public bool Success { get; set; }
        public long NewPlayMult { get; set; }
    }
}


