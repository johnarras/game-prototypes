using MessagePack;
using Genrpg.Shared.Website.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.PlayMultiplier.WebApi
{
    public class SetPlayMultRequest : IClientUserRequest
    {
        public long PlayMult { get; set; }
    }
}


