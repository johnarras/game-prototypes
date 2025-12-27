using MessagePack;
using Genrpg.Shared.Website.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Users.WebApi
{
    public class UpdateClientUserResponse : IWebResponse
    {
        public long Level { get; set; }
    }
}


