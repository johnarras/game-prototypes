using Genrpg.Editor.Importers.Core;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils;

namespace Genrpg.Editor.Importers.Purchasing
{
    /// <summary>
    /// Used to import an entire set of default stores for the player (needs ab test id)
    /// </summary>
    public abstract class BaseStoreOfferImporter<TParent, TChild> : ParentChildImporter<TParent, TChild> where TParent : ParentSettings<TChild> where TChild : ChildSettings, IIdName, new()
    {
        protected override bool IsIncrementalImporter()
        {
            return true;
        }
        protected ITextSerializer _serializer = null;
    }
}
