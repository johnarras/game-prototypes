using MessagePack;
using Genrpg.Shared.Website.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.BoardGame.Markers.WebApi
{
    [MessagePackObject]
    public class SetMarkerRequest : IClientUserRequest
    {
        [Key(0)] public long MarkerId { get; set; }
        [Key(1)] public long MarkerTier { get; set; }
    }
}
