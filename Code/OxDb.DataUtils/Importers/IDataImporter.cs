using OxDb.DataUtils.Entities.Core;
using OxDb.SharedCore.Interfaces;

namespace OxDb.DataUtils.Importers
{
    public interface IDataImporter : ISetupDictionaryItem<Type>
    {
        string ImportDataFilename { get; }

        Task<bool> ImportData(EditorGameState gs);
    }
}


