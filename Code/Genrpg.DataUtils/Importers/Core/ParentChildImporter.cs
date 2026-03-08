using Genrpg.DataUtils.Entities.Core;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.Interfaces;

namespace Genrpg.DataUtils.Importers.Core
{
    public abstract class ParentChildImporter<TParent, TChild> : BaseDataImporter<TParent> where TParent : ParentSettings<TChild>, new() where TChild : ChildSettings, IIdName, new()
    {
        protected abstract void ImportChildSubObject(EditorGameState gs, TChild current, int row, string firstColumn, string[] headers, string[] rowWords);

        protected virtual bool IsIncrementalImporter() { return false; }

        protected override async Task<bool> ParseInputFromLines(EditorGameState gs, List<string[]> lines)
        {
            TParent currParent = null;

            List<TChild> fullChildList = new List<TChild>();
            List<TChild> newChildList = new List<TChild>();

            string parentTypeName = typeof(TParent).Name.ToLower();
            string childTypeName = typeof(TChild).Name.ToLower();

            List<string> childTypeNames = new List<string>();
            childTypeNames.Add(childTypeName);

            if (childTypeName.LastIndexOf("type") == childTypeName.Length -4)
            {
                childTypeNames.Add(childTypeName.Substring(0, childTypeName.LastIndexOf("type")));
            }

            Dictionary<string, string[]> headers = new Dictionary<string, string[]>();

            Dictionary<string, object> _contextObjects = new Dictionary<string, object>();

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

                if (rowWords[0] == parentTypeName)
                {
                    if (currParent != null)
                    {
                        currParent.SetData(fullChildList);
                        gs.LookedAtObjects.AddRange(newChildList);
                        gs.LookedAtObjects.Add(currParent);
                        gs.data.Set(currParent);
                        currParent = null;
                        newChildList = new List<TChild>();
                        fullChildList = new List<TChild>();
                    }
                    currParent = _importService.ImportLine<TParent>(gs, row, headers[parentTypeName], rowWords);
                    fullChildList = currParent.GetData().ToList();
                }
                else if (childTypeNames.Contains(rowWords[0]))
                {
                    if (currParent == null)
                    {
                        currParent = gs.data.Get<TParent>(null);
                        fullChildList = currParent.GetData().ToList();
                    }

                    currentChild = _importService.ImportLine<TChild>(gs, row, headers[childTypeName], rowWords);

                    TChild existingChild = fullChildList.FirstOrDefault(x => x.IdKey == currentChild.IdKey);

                    if (existingChild != null)
                    {
                        fullChildList.Remove(existingChild);
                    }

                    if (newChildList.Any(x=>x.IdKey == currentChild.IdKey))
                    {
                        ShowErrorDialog(gs, "Duplicate object with Idkey: " + currentChild.IdKey);
                        return false;
                    }

                    fullChildList.Add(currentChild);
                    newChildList.Add(currentChild);
                    currParent.SetData(fullChildList);
                }
                else
                {
                    if (headers.TryGetValue(rowWords[0].ToLower(), out string[] headerRow))
                    {
                        ImportChildSubObject(gs, currentChild, row, rowWords[0].ToLower(), headerRow, rowWords);
                    }
                }
            }
            if (currParent != null)
            {
                currParent.SetData(fullChildList);
                gs.LookedAtObjects.AddRange(newChildList);
                gs.LookedAtObjects.Add(currParent);
                gs.data.Set(currParent);
            }
            await Task.CompletedTask;
            return true;
        }
    }
}
