using Genrpg.Shared.Editors.Interfaces;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;

namespace Genrpg.Shared.GameSettings.Interfaces
{
    public interface IGameSettings : IStringId, IEditorMetaDataTarget
    {
        void SetInternalIds();
        void ClearIndex();
        List<IGameSettings> GetChildren();
        DateTime SaveTime { get; set; }
    }
}


