using MessagePack;
using Genrpg.Shared.Website.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Accounts.WebApi.NewVersions
{
    [MessagePackObject]
    public class NewVersionResponse : IWebResponse
    {
        [Key(0)] public string MinNewClientVersion { get; set; }
    }
}
