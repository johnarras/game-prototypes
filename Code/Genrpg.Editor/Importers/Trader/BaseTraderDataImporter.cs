using Genrpg.Editor.Entities.Core;
using Genrpg.Editor.Importers.Core;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Interfaces;
using System.Threading.Tasks;

namespace Genrpg.Editor.Importers.Trader
{
    public abstract class BaseTraderDataImporter<TParent, TChild> : ParentChildImporter<TParent, TChild> where TParent : ParentSettings<TChild> where TChild : ChildSettings, IIdName, new()
    {
        protected override async Task<bool> UpdateAfterImport(WindowBase win, EditorGameState gs)
        {

            await Task.CompletedTask;
            return true;
        }
    }
}
