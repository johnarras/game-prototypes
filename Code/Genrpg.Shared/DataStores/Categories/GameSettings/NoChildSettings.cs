using Genrpg.Shared.GameSettings.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.DataStores.Categories.GameSettings
{
    public abstract class NoChildSettings : TopLevelGameSettings
    {
        public override ITopLevelSettings Unpack() { return this; }
    }
}
