using MessagePack;
using Genrpg.Shared.Website.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Accounts.WebApi.NewVersions
{
    public class NewVersionResponse : IWebResponse
    {
        public string MinNewClientVersion { get; set; }
    }
}


