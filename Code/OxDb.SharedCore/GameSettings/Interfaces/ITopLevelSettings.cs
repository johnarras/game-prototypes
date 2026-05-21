using System.Collections.Generic;

namespace OxDb.SharedCore.GameSettings.Interfaces
{
    public interface ITopLevelSettings : IGameSettings
    {
        ITopLevelSettings Unpack();
        void SetupForEditor(List<object> saveList);
    }
}


