using MessagePack;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Genrpg.Shared.GameSettings.Interfaces
{
    public interface ITopLevelSettings : IGameSettings
    {
        ITopLevelSettings Unpack();
        void SetupForEditor(List<object> saveList);
    }
}


