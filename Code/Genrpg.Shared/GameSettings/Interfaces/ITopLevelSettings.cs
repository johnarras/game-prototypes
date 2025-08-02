
using Genrpg.Shared.Settings.Settings;
using MessagePack;
using System;
using System.Collections.Generic;

namespace Genrpg.Shared.GameSettings.Interfaces
{
    [Union(0, typeof(SettingsNameSettings))]
    public interface ITopLevelSettings : IGameSettings
    {
        DateTime SaveTime { get; set; }
        ITopLevelSettings Unpack();
        void SetupForEditor(List<object> saveList);
    }
}
