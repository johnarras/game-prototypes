using Genrpg.Shared.Crawler.Info.Services;
using Genrpg.Shared.Crawler.Stats.Services;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Entities.Services;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Genrpg.Shared.Crawler.Info.InfoHelpers
{
    public abstract class BaseInfoHelper<TParent, TChild> : IInfoHelper where TParent : ParentSettings<TChild> where TChild : ChildSettings, IIndexedGameItem, new()
    {

        protected IGameData _gameData;
        protected IClientGameState _gs;
        protected ICrawlerStatService _statService;
        protected IEntityService _entityService;
        protected IInfoService _infoService;


        public abstract long Key { get; }

        virtual protected bool MakeNamePlural() { return true; }

        public virtual List<string> GetInfoLines(long entityId)
        {
            TChild child = _gameData.Get<TParent>(_gs.ch).Get(entityId);

            List<string> lines = new List<string>();

            if (child != null)
            {
                lines.Add(_infoService.CreateHeaderLine(child.Name, MakeNamePlural()));

                if (child is IIndexedGameItem indexedItem && !string.IsNullOrEmpty(indexedItem.Desc))
                {
                    lines.Add("Desc: " + indexedItem.Desc);
                }
            }

            return lines;
        }
        public virtual string GetTypeName() { return typeof(TChild).Name; }

        protected virtual bool IsValidInfoChild(TChild child) { return true; }

        public List<IIdName> GetInfoChildren()
        {
            TParent parent = _gameData.Get<TParent>(_gs.ch);

            return parent.GetData().Where(x => IsValidInfoChild(x)).Cast<IIdName>().ToList();
        }
    }
}
