using MessagePack;
using Genrpg.Shared.Website.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Users.WebApi
{
    [MessagePackObject]
    public class UpdateClientUserResponse : IWebResponse
    {
        [Key(0)] public long Level { get; set; }
    }
}
