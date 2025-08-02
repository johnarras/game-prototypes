using Genrpg.Editor.Constants;
using Genrpg.Editor.Entities.Core;
using Genrpg.Shared.Interfaces;
using System.Threading.Tasks;

namespace Genrpg.Editor.Importers
{
    public interface IDataImporter : ISetupDictionaryItem<EImportTypes>
    {
        string ImportDataFilename { get; }

        Task<bool> ImportData(WindowBase window, EditorGameState gs);
    }
}
