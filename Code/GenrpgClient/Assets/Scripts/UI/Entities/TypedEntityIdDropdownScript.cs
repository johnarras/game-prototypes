using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.UI.Entities
{
    public abstract class TypedEntityIdDropdownScript<TParent, TChild> : EntityIdDropdownList where TParent : ParentSettings<TChild> where TChild : ChildSettings, IIdName, new()
    {
        public override List<IIdName> GetChildList(IGameData gameData)
        {
            return gameData.Get<TParent>(null).GetData().Cast<IIdName>().ToList();
        }
    }
}
