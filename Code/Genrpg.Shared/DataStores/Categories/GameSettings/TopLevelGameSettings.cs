using Genrpg.Shared.GameSettings;
using Genrpg.Shared.GameSettings.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.DataStores.Categories.GameSettings
{
    public abstract class TopLevelGameSettings : BaseGameSettings, ITopLevelSettings
    {
        public abstract ITopLevelSettings Unpack();

        public virtual void SetupForEditor(List<object> saveList)
        {

        }
    }
}
