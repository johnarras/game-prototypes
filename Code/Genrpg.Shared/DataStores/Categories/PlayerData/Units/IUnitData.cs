using Genrpg.Shared.CharMail.PlayerData;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Editors.Interfaces;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Units.Entities;
using MessagePack;

namespace Genrpg.Shared.DataStores.Categories.PlayerData.Units
{
    public interface IUnitData : IStringId, IEditorMetaDataTarget
    {
        IUnitData Unpack();
    }
}
