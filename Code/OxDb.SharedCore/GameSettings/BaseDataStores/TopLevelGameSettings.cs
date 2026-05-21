using OxDb.SharedCore.GameSettings.Interfaces;
using System.Collections.Generic;

namespace OxDb.SharedCore.GameSettings.BaseDataStores
{
    public abstract class TopLevelGameSettings : BaseGameSettings, ITopLevelSettings
    {
        public abstract ITopLevelSettings Unpack();

        public virtual void SetupForEditor(List<object> saveList)
        {

        }
    }
}


