using MessagePack;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Spells.Casting
{
    // MessagePackIgnore
    public class TargetCastState
    {
        public Unit Target { get; set; }
        public TryCastState State { get; set; }
    }
}
