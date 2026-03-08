using Genrpg.DataUtils.Entities.Core;
using Genrpg.Shared.Interfaces;
using System;
using System.Threading.Tasks;

namespace Genrpg.DataUtils.Importers
{
    public interface IDataImporter : ISetupDictionaryItem<Type>
    {
        string ImportDataFilename { get; }

        Task<bool> ImportData(EditorGameState gs);
    }
}


