using OxDb.DataUtils.Entities.Core;
using OxDb.DataUtils.Services.Importing;
using OxDb.SharedCore.DataStores.Interfaces;
using OxDb.SharedCore.Effects.Entities;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.GameSettings.Interfaces;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Attributes.Services;
using OxDb.SharedGame.Spells.Settings.Elements;
using System.Reflection;

namespace OxDb.DataUtils.Importers.Core
{
    public abstract class BaseRawDataImporter : IDataImporter
    {

        protected ILogService _logService = null;
        protected IRepositoryService _repoService = null;
        protected IImportService _importService = null;
        protected IAttributeService _attributeService = null;
        protected IReflectionService _reflectionService = null;
        protected IGameData _gameData = null;

        public abstract string ImportDataFilename { get; }

        public abstract Type HelperKey { get; }

        protected abstract Task<bool> ParseInputFromLines(EditorGameState gs, List<string[]> lines);

        protected virtual Task<bool> UpdateAfterImport(EditorGameState gs)
        {
            return Task.FromResult(true);
        }

        string dataFolderOffsetPath = "\\..\\..\\..\\..\\..\\..\\..\\ImportData\\";
        protected List<string[]> ReadImportDataLines(string importFilename)
        {
            string strExeFilePath = Assembly.GetExecutingAssembly().Location;

            int lastSlashIndex = strExeFilePath.LastIndexOf("\\");

            strExeFilePath = strExeFilePath.Substring(0, lastSlashIndex);

            string fullFilePath = strExeFilePath + dataFolderOffsetPath + importFilename;

            string text = File.ReadAllText(fullFilePath);

            List<string> lines = StrUtils.SplitIntoLines(text);

            for (int l = 0; l < lines.Count; l++)
            {

                lines[l] = StrUtils.SanitizeSingleEnglishLine(lines[l].Trim());
            }

            List<string[]> retval = new List<string[]>();

            for (int l = 0; l < lines.Count; l++)
            {
                string[] words = StrUtils.SafeSplitCommaLine(lines[l]);
                retval.Add(words);
            }

            return retval;
        }

        public async Task<bool> ImportData(EditorGameState gs)
        {
            try
            {
                List<string[]> lines = ReadImportDataLines(ImportDataFilename);

                if (lines.Count < 1)
                {
                    return false;
                }

                if (!await ParseInputFromLines(gs, lines) ||
                    !await UpdateAfterImport(gs))
                {
                    gs.LookedAtObjects = new List<object>();
                }
                List<object> lookedAtObjects = new List<object>(gs.LookedAtObjects);
                foreach (object lookedAtObject in lookedAtObjects)
                {
                    if (lookedAtObject is ITopLevelSettings topLevel)
                    {
                        topLevel.SetupForEditor(gs.LookedAtObjects);
                    }
                }
            }
            catch (Exception ex)
            {
                _logService.Exception(ex, "Import Data From: " + ImportDataFilename);
                throw ex;

            }


            await Task.CompletedTask;
            return true;
        }


        protected void ShowErrorDialog(EditorGameState gs, string message)
        {
            gs.LookedAtObjects.Clear();

            throw new Exception(message);
        }


        public List<Effect> ReadElementWords(string wordList, long entityTypeId, IReadOnlyList<ElementType> elementTypes)
        {
            List<Effect> retval = new List<Effect>();
            if (string.IsNullOrEmpty(wordList))
            {
                return retval;
            }

            string[] words = wordList.Split(' ');

            for (int w = 0; w < words.Length; w++)
            {
                string word = words[w].ToLower().Replace("_", "");

                ElementType etype = elementTypes.FirstOrDefault(x => StrUtils.IsLowercaseEqual(x.Name, word));

                if (etype != null)
                {
                    retval.Add(new Effect() { EntityTypeId = entityTypeId, EntityId = etype.IdKey, Quantity = 1 });
                }
                else
                {
                    _logService.Error("Missing element called: " + word);
                }
            }

            return retval;
        }

    }
}


