using MessagePack;
using Genrpg.Shared.Website.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.BoardGame.Markers.WebApi
{
    [MessagePackObject]
    public class SetMarkerResponse : IWebResponse
    {
        [Key(0)] public long MarkerId { get; set; }
        [Key(1)] public long MarkerTier { get; set; }
        [Key(2)] public bool Success { get; set; }
    }
}
