
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.GameSettings.Interfaces;
using MessagePack;
using System.Collections.Generic;

namespace Genrpg.Shared.DataStores.Categories.GameSettings
{
    [MessagePackObject]
    public class ParentSettingsDto<TParent, TChild> : StubGameSettings, ITopLevelSettings
        where TParent : ParentSettings<TChild>, new()
        where TChild : ChildSettings, new()
    {
        [Key(0)] public override string Id { get; set; }
        [Key(1)] public List<TChild> Children { get; set; } = new List<TChild>();
        [Key(2)] public TParent Parent { get; set; }

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
