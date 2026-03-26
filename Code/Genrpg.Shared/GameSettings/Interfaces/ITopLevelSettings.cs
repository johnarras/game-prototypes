using System.Collections.Generic;

namespace Genrpg.Shared.GameSettings.Interfaces
{
    public interface ITopLevelSettings : IGameSettings
    {
        ITopLevelSettings Unpack();
        void SetupForEditor(List<object> saveList);
    }
}


