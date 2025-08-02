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
    public class FirstLetterFindRiddleTypeHelper : BaseRiddleTypeHelper
    {
        public override long Key => RiddleTypes.FirstLetter;

        protected override async Task<bool> AddRiddleInternal(RiddleLookup lookup, CrawlerMap lockedFloor, CrawlerMap prevFloor, List<PointXZ> openPoints, IRandom rand)
        {
            Dictionary<char, List<string>> dict = lookup.LetterPositionWords[0];

            List<char> keys = dict.Keys.ToList();

            int wordCount = 7;

            List<char> chosenLetters = new List<char>();

            for (int i = 0; i < wordCount; i++)
            {
                char c = keys[rand.Next() % keys.Count];

                keys.Remove(c);
                chosenLetters.Add(c);
            }

            List<string> riddleWords = new List<string>();

            chosenLetters = chosenLetters.OrderBy(x => x).ToList();

            int middlePos = wordCount / 2;
            char middle = chosenLetters[middlePos];
            char prev = chosenLetters[middlePos - 1];
            char next = chosenLetters[middlePos + 1];

            foreach (char c in chosenLetters)
            {
                List<string> words = dict[c];

                riddleWords.Add(words[rand.Next() % words.Count]);
            }

            string answer = riddleWords[middlePos];
            riddleWords[middlePos] = "????????";

            List<char> badAnswerChars = keys.Where(x => x < prev || x > next).ToList();

            int wrongAnswerCount = 3;

            List<string> otherAnswers = new List<string>();

            for (int i = 0; i < wrongAnswerCount; i++)
            {
                char badLetter = badAnswerChars[rand.Next() % badAnswerChars.Count];

                otherAnswers.Add(dict[badLetter][rand.Next() % dict[badLetter].Count]);
            }

            otherAnswers.Insert(rand.Next() % otherAnswers.Count, answer);

            StringBuilder riddleText = new StringBuilder();

            riddleText.Append("Select the word that fits properly within the following sequence:\n\n");

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

            await Task.CompletedTask;
            return true;
        }
    }
}
