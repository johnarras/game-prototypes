using Genrpg.Shared.Crawler.Maps.Constants;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Riddles.Constants;
using Genrpg.Shared.Riddles.Entities;
using Genrpg.Shared.Riddles.Settings;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Utils.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.Shared.Riddles.EntranceRiddleHelpers
{
    public class BasicRiddleTypeHelper : BaseRiddleTypeHelper
    {
        public override long Key => RiddleTypes.Basic;

        protected override async Task<bool> AddRiddleInternal(RiddleLookup lookup, CrawlerMap lockedFloor, CrawlerMap prevFloor, List<PointXZ> openPoints, IRandom rand)
        {
            await Task.CompletedTask;
            IReadOnlyList<Riddle> riddles = _gameData.Get<RiddleSettings>(_gs.ch).GetData();

            if (riddles.Count < 1)
            {
                return false;
            }

            StringBuilder riddleText = new StringBuilder();
            riddleText.Append("Answer the following to pass:\n\n");

            Riddle riddle = riddles[rand.Next(riddles.Count)];

            string[] lines = riddle.Desc.Split('\n');

            List<int> startIndexes = new List<int>();

            int nonEmptyLineCount = 0;
            for (int l = 0; l < lines.Length; l++)
            {

                if (!string.IsNullOrEmpty(lines[l]))
                {
                    startIndexes.Add(l);
                    nonEmptyLineCount++;
                }
            }

            List<int> endIndexes = new List<int>();

            while (startIndexes.Count > 0)
            {
                int index = startIndexes[rand.Next(startIndexes.Count)];
                endIndexes.Add(index);
                startIndexes.Remove(index);
            }

            int nonEmptyIndex = 0;
            for (int l = 0; l < lines.Length; l++)
            {
                if (!String.IsNullOrEmpty(lines[l]))
                {
                    riddleText.Append(lines[l].Substring(0, 3) + ".......\n");
                    StringBuilder clueText = new StringBuilder();

                    clueText.Append("Some strange writing is here...\n\n");

                    clueText.Append(lines[l] + "\n\n");

                    int currIndex = endIndexes[nonEmptyIndex++];
                    for (int i = 0; i < riddle.Name.Length; i++)
                    {
                        if ((i % nonEmptyLineCount) == currIndex)
                        {
                            clueText.Append(riddle.Name[i]);
                        }
                        else
                        {
                            clueText.Append("?");
                        }
                    }
                    clueText.Append("\n\n");

                    PointXZ openPoint = openPoints[rand.Next(openPoints.Count)];
                    openPoints.Remove(openPoint);
                    prevFloor.SetEntity(openPoint.X, openPoint.Z, EntityTypes.Riddle, l + 1);
                    prevFloor.RiddleHints.Hints.Add(new RiddleHint() { Index = l+1, Text = clueText.ToString() });
                }
            }

            riddleText.Append("\n\nWhat am I?\n\n");

            riddleText.Append("\n");
            for (int i = 0; i < riddle.Name.Length; i++)
            {
                riddleText.Append("?");
            }

            lockedFloor.RemoveFlags(CrawlerMapFlags.ShowFullRiddleText);

            lockedFloor.EntranceRiddle.Text = riddleText.ToString();
            lockedFloor.EntranceRiddle.Answer = riddle.Name.ToLower().Trim();
            lockedFloor.EntranceRiddle.Error = "Sorry, that is not correct. Look around for clues...";
            return true;
        }
    }
}
