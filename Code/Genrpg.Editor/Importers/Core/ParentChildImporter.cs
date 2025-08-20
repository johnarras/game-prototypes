using Genrpg.Editor.Entities.Core;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Genrpg.Editor.Importers.Core
{
    public abstract class ParentChildImporter<TParent, TChild> : BaseDataImporter where TParent : ParentSettings<TChild> where TChild : ChildSettings, IIdName, new()
    {
        protected abstract void ImportChildSubObject(EditorGameState gs, TChild current, int row, string firstColumn, string[] headers, string[] rowWords);

        protected virtual bool IsIncrementalImporter() { return false; }

        protected override async Task<bool> ParseInputFromLines(WindowBase window, EditorGameState gs, List<string[]> lines)
        {
            TParent settings = gs.data.Get<TParent>(null);

            List<TChild> fullList = new List<TChild>();
            List<TChild> newList = new List<TChild>();

            // In incemental importer, we have full list already there to set in data.
            if (IsIncrementalImporter())
            {
                fullList = settings.GetData().ToList();
            }

            string childTypeName = typeof(TChild).Name.ToLower();
            Dictionary<string, string[]> headers = new Dictionary<string, string[]>();

            TChild currentChild = null;
            for (int row = 0; row < lines.Count; row++)
            {
                string[] rowWords = lines[row];

                if (rowWords.Length < 2 || string.IsNullOrEmpty(rowWords[0]))
                {
                    continue;
                }

                rowWords[0] = rowWords[0].ToLower();

                if (rowWords[0].IndexOf("header") >= 0)
                {
                    string headerWord = rowWords[0].Replace("header", "").Trim();

                    headers[headerWord] = rowWords;
                    continue;
                }


                if (rowWords[0] == childTypeName)
                {
                    currentChild = _importService.ImportLine<TChild>(gs, row, rowWords, headers[childTypeName]);

                    TChild existingChild = fullList.FirstOrDefault(x => x.IdKey == currentChild.IdKey);

                    if (existingChild != null)
                    {
                        fullList.Remove(existingChild);
                    }

                    fullList.Add(currentChild);
                    newList.Add(currentChild);
                }
                else
                {
                    if (headers.TryGetValue(rowWords[0].ToLower(), out string[] headerRow))
                    {
                        ImportChildSubObject(gs, currentChild, row, rowWords[0].ToLower(), headerRow, rowWords);
                    }
                }
            }


            settings.SetData(fullList);
            gs.LookedAtObjects.AddRange(newList);
            gs.LookedAtObjects.Add(settings);
            await Task.CompletedTask;
            return true;
        }
    }
}
