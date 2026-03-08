using Genrpg.DataUtils.Entities.Core;
using Genrpg.Shared.GameSettings.Interfaces;

namespace Genrpg.DataUtils.Importers.Core
{
    public abstract class NoChildImporter<TParent> : BaseDataImporter<TParent> where TParent : class, ITopLevelSettings, new()
    {
        /// <summary>
        /// A little strange, but nochild objects MIGHT have very small child lists of objects that aren't really full child objects.
        /// </summary>
        /// <param name="gs"></param>
        /// <param name="current"></param>
        /// <param name="row"></param>
        /// <param name="firstColumn"></param>
        /// <param name="headers"></param>
        /// <param name="rowWords"></param>
        protected abstract void ImportNoChildSubObject(EditorGameState gs, TParent current, int row, string firstColumn, string[] headers, string[] rowWords);

        protected override async Task<bool> ParseInputFromLines(EditorGameState gs, List<string[]> lines)
        {
            TParent currParent = null;

            string parentTypeName = typeof(TParent).Name.ToLower();
            Dictionary<string, string[]> headers = new Dictionary<string, string[]>();

            Dictionary<string, object> _contextObjects = new Dictionary<string, object>();

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
                    currParent = _importService.ImportLine<TParent>(gs, row, headers[parentTypeName], rowWords);
                    gs.data.Set(currParent);
                    gs.LookedAtObjects.Add(currParent);
                }
                else
                {
                    if (headers.ContainsKey(rowWords[0]))
                    {
                        ImportNoChildSubObject(gs, currParent, row, rowWords[0], headers[rowWords[0]], rowWords);
                    }
                }
            }
            await Task.CompletedTask;
            return true;
        }
    }
}
