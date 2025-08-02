using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Riddles.Constants;
using Genrpg.Shared.Utils.Data;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Genrpg.Shared.Crawler.Maps.Constants;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Riddles.Entities;

namespace Genrpg.Shared.Riddles.EntranceRiddleHelpers
{
    public class PositionWordFindRiddleTypeHelper : BaseRiddleTypeHelper
    {
        public override long Key => RiddleTypes.PositionWordFind;

        protected override async Task<bool> AddRiddleInternal(RiddleLookup lookup, CrawlerMap lockedFloor, CrawlerMap prevFloor, List<PointXZ> openPoints, IRandom rand)
        {
            await Task.CompletedTask;

            string wordChosen = lookup.AllWords[rand.Next(lookup.AllWords.Count)];


            List<MapCellDetail> clueDetails = new List<MapCellDetail>();

            List<string> clueWords = new List<string>();


            for (int l = 0; l < wordChosen.Length; l++)
            {

                List<string> wordChoices = new List<string>();
                int startOffsetIndex = MathUtils.IntRange(0, RiddleConstants.MaxLetterPosition, rand);

                if (rand.NextDouble() < 0.7f)
                {
                    startOffsetIndex /= 2;
                }

                string okWord = null;

                string offsetName = lookup.OffsetWords[startOffsetIndex];


                for (int idx = startOffsetIndex; idx >= 0; idx--)
                {

                    if (lookup.LetterPositionWords.ContainsKey(idx))
                    {
                        continue;
                    }

                    Dictionary<char, List<string>> offsetDict = lookup.LetterPositionWords[idx];

                    if (offsetDict.TryGetValue(wordChosen[l], out List<string> words))
                    {
                        if (words.Count < 1)
                        {
                            continue;
                        }

                        for (int times = 0; times < 4; times++)
                        {
                            okWord = words[rand.Next(words.Count)];

                            if (okWord == wordChosen || clueWords.Contains(okWord))
                            {
                                okWord = null;
                                continue;
                            }
                            else
                            {
                                break;
                            }
                        }

                        if (okWord != null)
                        {
                            offsetName = lookup.OffsetWords[idx];
                            break;
                        }
                    }
                }

                if (okWord == null)
                {
                    return false;
                }

                clueWords.Add(okWord);

                StringBuilder clueText = new StringBuilder();

                clueText.Append("A word is written in blood:\n\n");

                clueText.Append(okWord + "\n\n");

                clueText.Append($"The {offsetName} letter is heavily scratched....\n\n");

                PointXZ openPoint = openPoints[rand.Next(openPoints.Count)];
                openPoints.Remove(openPoint);

                prevFloor.SetEntity(openPoint.X, openPoint.Z, EntityTypes.Riddle, l + 1);
                prevFloor.RiddleHints.Hints.Add(new RiddleHint() { Index = l + 1, Text = clueText.ToString() });
            }

            if (clueDetails.Count != wordChosen.Length)
            {
                return false;
            }

            prevFloor.Details.AddRange(clueDetails);

            StringBuilder riddleText = new StringBuilder();

            riddleText.Append("You must speak the proper word to pass.\n\n");

            riddleText.Append("Search these corridors for the scrawls\n\n");

            riddleText.Append("of madmen who failed to answer the riddle.\n\n");

            riddleText.Append($"Hint: The answer has {wordChosen.Length} letters.\n\n");

            lockedFloor.AddFlags(CrawlerMapFlags.ShowFullRiddleText);
            lockedFloor.EntranceRiddle.Text = riddleText.ToString();
            lockedFloor.EntranceRiddle.Answer = wordChosen;
            lockedFloor.EntranceRiddle.Error = "Sorry, that is not the proper word to pass...";
            return true;
        }
    }
}
