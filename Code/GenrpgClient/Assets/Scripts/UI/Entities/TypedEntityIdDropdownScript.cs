using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace OxDb.Client.UI.Entities
{
    public abstract class TypedEntityIdDropdownScript<TParent, TChild> : EntityIdDropdownList where TParent : ParentSettings<TChild> where TChild : ChildSettings, IIdName, new()
    {
        public override List<IIdName> GetChildList(IGameData gameData)
        {
            return gameData.Get<TParent>(null).GetData().Cast<IIdName>().ToList();
        }
    }
}


