using Genrpg.Shared.Client.Interfaces;
using MessagePack;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Genrpg.Shared.Website.Interfaces
{
    public interface IWebResponse : IWebMessage, IClientEvent
    {
    }
}


