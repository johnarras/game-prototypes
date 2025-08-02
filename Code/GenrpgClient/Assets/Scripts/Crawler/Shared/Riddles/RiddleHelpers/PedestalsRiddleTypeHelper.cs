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
using Genrpg.Shared.Entities.Constants;
using System.Linq;
using UnityEngine;
using Genrpg.Shared.Logging.Interfaces;
using Steamworks;

namespace Genrpg.Shared.Riddles.EntranceRiddleHelpers
{
    public class PedestalIndex
    {
        public int Index { get; set; }
        public string Value { get; set; }
        public bool IsValid { get; set; }
      
    }


    /// <summary>
    /// Set up pedestal riddles. The idea is to have N pedestals and have a condition and
    /// make the pedestals have to be on or off 
    /// </summary>
    public class PedestalsRiddleTypeHelper : BaseRiddleTypeHelper
    {
        public override long Key => RiddleTypes.Pedestals;

        protected ILogService _logService = null;

        protected override async Task<bool> AddRiddleInternal(RiddleLookup lookup, CrawlerMap lockedFloor, CrawlerMap prevFloor, List<PointXZ> startOpenPoints, IRandom rand)
        {
            int pedestalCount = rand.Next(3, 5);

            List<PointXZ> horizPoints = new List<PointXZ>();
            List<PointXZ> vertPoints = new List<PointXZ>();
            foreach (PointXZ point in startOpenPoints)
            {
                if (IsOkStartingPoint(prevFloor, point.X, point.Z, startOpenPoints, pedestalCount, false))
                {
                    horizPoints.Add(point);
                }
                if (IsOkStartingPoint(prevFloor, point.X, point.Z, startOpenPoints, pedestalCount, true))
                {
                    vertPoints.Add(point);
                }
            }

            PointXZ startPoint = null;
            bool isVertical = false;

            if (rand.NextDouble() < 0.5)
            {
                if (horizPoints.Count < 1)
                {
                    return false;
                }

                startPoint = horizPoints[rand.Next(horizPoints.Count)];
                isVertical = false;
            }
            else
            {
                if (vertPoints.Count < 1)
                {
                    return false;
                }

                startPoint = vertPoints[rand.Next(vertPoints.Count)];
                isVertical = true;
            }

            if (startPoint == null)
            {
                return false;
            }

            List<PointXZ> finalPoints = new List<PointXZ>();

            if (isVertical)
            {
                for (int i = 0; i < pedestalCount; i++)
                {
                    finalPoints.Add(new PointXZ(startPoint.X, startPoint.Z + i));
                }
            }
            else
            {
                for (int i = 0; i < pedestalCount; i++)
                {
                    finalPoints.Add(new PointXZ(startPoint.X+i, startPoint.Z)); 
                }
            }

            if (rand.NextDouble() < 0.5f)
            {
                finalPoints.Reverse();
            }

            List<PedestalIndex> indexes = new List<PedestalIndex>();

            string hintString = "";

            bool isLikePedestal = false;

            if (rand.NextDouble() < 0.5f)
            {
                int divSize = 2;
                bool isMult = false;

                if (true || rand.NextDouble() < 0.3f)
                {
                    divSize = 2;
                    if (rand.NextDouble() < 0.5f)
                    {
                        hintString = "I am rather even-tempered.";
                        isMult = true;
                    }
                    else
                    {
                        hintString = "I am quite the odd duck.";
                        isMult = false;
                    }
                    divSize = 2;
                }

                for (int i = 0; i < pedestalCount; i++)
                {
                    int value = rand.Next(0, 25);

                    // Want multiple.
                    if (rand.NextDouble() < 0.5f)
                    {
                        value *= divSize;
                    }
                    else
                    {
                        if (value % divSize == 0)
                        {
                            value += rand.Next(1, divSize - 1);
                        }
                    }

                    PedestalIndex index = new PedestalIndex()
                    {
                        Index = i,
                        Value = value.ToString(),
                        IsValid = (value % divSize == 0) == isMult,
                    };
                    indexes.Add(index);
                }
                isLikePedestal = isMult;
            }
            else 
            {
                isLikePedestal = true;
                long weightTotal = lookup.WordsContainingLetters.Values.Sum(x => x.Count);

                long valueChosen = MathUtils.LongRange(0, weightTotal, rand);

                char chosenLetter = '1';

                foreach (char c in lookup.WordsContainingLetters.Keys)
                {
                    valueChosen -= lookup.WordsContainingLetters[c].Count;

                    if (valueChosen <= 0)
                    {
                        chosenLetter = c;
                        break;
                    }
                }

                List<string> allGoodWords = lookup.WordsContainingLetters[chosenLetter];
                List<string> allBadWords = lookup.WordsNotContainingLetters[chosenLetter];

                if (allGoodWords.Count < 10 || allBadWords.Count < 10)
                {
                    return false;
                }

                int exampleWordCount = 4;

                List<string> goodExampleWords = new List<string>();

                int[] lettersUsedCounts = new int[26];
                while (goodExampleWords.Count < exampleWordCount)
                {
                    string newWord = allGoodWords[rand.Next(allGoodWords.Count)];
                    if (!goodExampleWords.Contains(newWord))
                    {
                        goodExampleWords.Add(newWord);
                    }

                    List<char> usedLetters = new List<char>();  
                    for (int c = 0; c < newWord.Length; c++)
                    {
                        char lowerChar = char.ToLower(newWord[c]);
                        if (!usedLetters.Contains(lowerChar))
                        {
                            usedLetters.Add(lowerChar);
                            lettersUsedCounts[(int)(lowerChar - 'a')]++;
                        }
                    }
                }

                List<char> otherLettersFullyUsed = new List<char>();
                for (int i = 0; i < 26; i++)
                {
                    if (lettersUsedCounts[i] == exampleWordCount)
                    {
                        if ((char)(i+'a') != chosenLetter)
                        {
                            otherLettersFullyUsed.Add((char)(i+'a'));
                        }
                    }
                }

                List<string> extraWords = allGoodWords.Except(goodExampleWords).ToList();

                if (otherLettersFullyUsed.Count > 0)
                {
                    foreach (char letter in otherLettersFullyUsed)
                    {
                        extraWords = extraWords.Where(x => !x.Contains(letter)).ToList();
                    }
                }

                goodExampleWords.Add(extraWords[rand.Next(extraWords.Count)]);

                List<string> badExampleWords = new List<string>();

                while (badExampleWords.Count < exampleWordCount+1)
                {
                    string badExample = allBadWords[rand.Next(allBadWords.Count)];  
                    if (!badExampleWords.Contains(badExample))
                    {
                        badExampleWords.Add(badExample);
                    }
                }

                goodExampleWords = goodExampleWords.OrderBy(x=>HashUtils.NewUUId()).ToList();    
                badExampleWords = badExampleWords.OrderBy(x=>HashUtils.NewUUId()).ToList();  

                StringBuilder sb = new StringBuilder();
                sb.Append("I like these things:\n");
                foreach (string word in goodExampleWords)
                {
                    sb.Append(word + " ");
                }
                sb.Append("\n");
                sb.Append("I dislike these things:\n");
                foreach (string word in badExampleWords)
                {
                    sb.Append(word + " ");
                }
                sb.Append("\n");
                hintString = sb.ToString(); 

                List<string> goodTestWords = allGoodWords.Except(goodExampleWords).ToList();    
                List<string> badTestWords = allBadWords.Except(badExampleWords).ToList();

                for (int i = 0; i < pedestalCount; i++)
                {
                    if (rand.NextDouble() < 0.5f)
                    {
                        PedestalIndex index = new PedestalIndex()
                        {
                            Index = i,
                            IsValid = true,
                            Value = goodTestWords[rand.Next(goodTestWords.Count)]
                        };
                        goodTestWords.Remove(index.Value);
                        indexes.Add(index);
                    }
                    else
                    {
                        PedestalIndex index = new PedestalIndex()
                        {
                            Index = i,
                            IsValid = false,
                            Value = badTestWords[rand.Next(badTestWords.Count)] 
                        };
                        badTestWords.Remove(index.Value);
                        indexes.Add(index);
                    }
                }
            }

            int riddleAnswer = 0;

            List<MapCellDetail> clueDetails = new List<MapCellDetail>();
            for (int i = 0; i < indexes.Count; i++)
            {
                
                PedestalIndex pedestal = indexes[i];

                int currIndex = i + 1;

                StringBuilder clueText = new StringBuilder();

                clueText.Append("A pedestal numbered " + (i+1) + "/" + indexes.Count + " is here.\n");
                clueText.Append("'" + pedestal.Value + "' is engraved on the pedestal.\n");
                clueText.Append("A mysterious voice says:\n");
                clueText.Append(hintString + "\n");
                if (isLikePedestal)
                {
                    clueText.Append("Activate precisely the orbs that I like.\n");
                }
                else
                {
                    clueText.Append("Activate only the orbs that I do not dislike.\n");
                }
                PointXZ openPoint = finalPoints[0];
                finalPoints.RemoveAt(0);
                prevFloor.Set(openPoint.X, openPoint.Z, CellIndex.Walls, 0);

                prevFloor.SetEntity(openPoint.X, openPoint.Z, EntityTypes.Riddle, currIndex);
                prevFloor.RiddleHints.Hints.Add(new RiddleHint() { Index = currIndex, Text = clueText.ToString() });
                
                if (pedestal.IsValid)
                {
                    riddleAnswer |= (1 << currIndex);
                }
            }

            prevFloor.Details.AddRange(clueDetails);

            StringBuilder riddleText = new StringBuilder();

            riddleText.Append("You must activate the orbs on this floor to pass.\n\n");

            riddleText.Append("Search this maze for the " + indexes.Count + " pedestals\n\n");

            if (isLikePedestal)
            {
                riddleText.Append("And only activate the orbs that I like.\n\n");
            }
            else
            {
                riddleText.Append("And only activate the orbs that do not displease me.\n\n");
            }

            lockedFloor.AddFlags(CrawlerMapFlags.ShowFullRiddleText);
            lockedFloor.EntranceRiddle.Text = riddleText.ToString();
            lockedFloor.EntranceRiddle.Answer = riddleAnswer.ToString();
            lockedFloor.EntranceRiddle.Error = "Sorry, the orbs are not correctly set...";
            await Task.CompletedTask;
            return true;
        }

        protected bool IsOkStartingPoint(CrawlerMap map, int sx, int sz, List<PointXZ> okPoints, int length, bool vertical)
        {
            int cx = sx;
            int cz = sz;
            int dx = (vertical ? 0 : 1);
            int dz = (vertical ? 1 : 0);

            for (int i = 0; i < length; i++)
            {
                if (!okPoints.Any(p => p.X == cx && p.Z == cz))
                {
                    return false;
                }

                for (int xx = cx - 1; xx <= cx + 1; xx++)
                {
                    if (xx <= 0 || xx >= map.Width - 1)
                    {
                        return false;
                    }

                    for (int zz = cz - 1; zz <= cz + 1; zz++)
                    {
                        if (zz <= 0 || zz >= map.Height - 1)
                        {
                            return false;
                        }

                        if (map.Get(xx, zz, CellIndex.Terrain) < 1)
                        {
                            return false;
                        }
                    }
                }
                cx += dx;
                cz += dz;
            }

            return true;
        }
    }
}
