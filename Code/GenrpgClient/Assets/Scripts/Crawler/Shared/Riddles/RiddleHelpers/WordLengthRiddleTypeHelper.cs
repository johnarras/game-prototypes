using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Riddles.Constants;
using Genrpg.Shared.Utils.Data;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Genrpg.Shared.Riddles.Entities;
using Genrpg.Shared.Crawler.Maps.Constants;
using System.Linq;

namespace Genrpg.Shared.Riddles.EntranceRiddleHelpers
{
    public class WordLengthRiddleTypeHelper : BaseRiddleTypeHelper
    {
        public override long Key => RiddleTypes.WordLength;

        protected override async Task<bool> AddRiddleInternal(RiddleLookup lookup, CrawlerMap lockedFloor, CrawlerMap prevFloor, List<PointXZ> openPoints, IRandom rand)
        {
            await Task.CompletedTask; Dictionary<int, List<string>> dict = lookup.WordsByLength;

            List<int> keys = dict.Keys.OrderBy(x => x).ToList();

            int wordCount = 5;

            List<int> chosenLengths = new List<int>();

            for (int i = 0; i < wordCount; i++)
            {
                int len = keys[rand.Next() % keys.Count];

                keys.Remove(len);
                chosenLengths.Add(len);
            }

            List<string> riddleWords = new List<string>();

            chosenLengths = chosenLengths.OrderBy(x => x).ToList();

            int middlePos = wordCount / 2;
            int middle = chosenLengths[middlePos];
            int prev = chosenLengths[middlePos - 1];
            int next = chosenLengths[middlePos + 1];

            foreach (int len in chosenLengths)
            {
                List<string> words = dict[len];

                riddleWords.Add(words[rand.Next() % words.Count]);
            }

            string answer = riddleWords[middlePos];

            riddleWords[middlePos] = "????????";

            List<int> badAnswerChars = keys.Where(x => x < prev || x > next).ToList();

            int wrongAnswerCount = 3;

            List<string> otherAnswers = new List<string>();

            for (int i = 0; i < wrongAnswerCount; i++)
            {
                if (badAnswerChars.Count < 1)
                {
                    return false;
                }

                int badLength = badAnswerChars[rand.Next() % badAnswerChars.Count];

                otherAnswers.Add(dict[badLength][rand.Next() % dict[badLength].Count]);
            }

            otherAnswers.Insert(rand.Next() % otherAnswers.Count, answer);

            StringBuilder riddleText = new StringBuilder();

            riddleText.Append("Select the word that fits best within the following sequence:\n\n");

            for (int i = 0; i < riddleWords.Count; i++)
            {
                riddleText.Append(riddleWords[i] + " ");
            }

            riddleText.Append("\n\n");

            riddleText.Append("Your choices are:\n\n");

            for (int i = 0; i < otherAnswers.Count; i++)
            {
                riddleText.Append(otherAnswers[i] + " ");
            }

            riddleText.Append("\n\n");

            riddleText.Append("Which one fits the best in the sequence?");

            lockedFloor.AddFlags(CrawlerMapFlags.ShowFullRiddleText);
            lockedFloor.EntranceRiddle.Text = riddleText.ToString();
            lockedFloor.EntranceRiddle.Answer = answer;
            lockedFloor.EntranceRiddle.Error = "Sorry, that is not the correct answer...";
            return true;
        }
    }
}
