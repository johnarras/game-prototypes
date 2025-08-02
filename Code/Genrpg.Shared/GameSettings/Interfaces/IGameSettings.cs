using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.DataStores.Interfaces;
using Genrpg.Shared.Editors.Interfaces;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.Shared.GameSettings.Interfaces
{
    public interface IGameSettings : IStringId, IEditorMetaDataTarget
    {
        void SetInternalIds();
        void ClearIndex();
        List<IGameSettings> GetChildren();
    }
}
