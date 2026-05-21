using OxDb.SharedCore.GameSettings.Interfaces;
using System.Collections.Generic;

namespace OxDb.SharedCore.GameSettings.BaseDataStores
{

    public abstract class ParentSettingsDto<TParent, TChild> : StubGameSettings, ITopLevelSettings
        where TParent : ParentSettings<TChild>, new()
        where TChild : ChildSettings, new()
    {
        [MessagePack.IgnoreMember] public abstract List<TChild> Children { get; set; }
        [MessagePack.IgnoreMember] public abstract TParent Parent { get; set; }

        public virtual ITopLevelSettings Unpack()
        {
            Parent.SetData(Children);
            return Parent;
        }

        public virtual void SetupForEditor(List<object> saveList)
        {

        }

    }
}


