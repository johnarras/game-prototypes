using Genrpg.Shared.ProcGen.Settings.Names;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Riddles.Entities
{
    public class RiddleLookup
    {
        public Dictionary<int, Dictionary<char, List<string>>> LetterPositionWords = new Dictionary<int, Dictionary<char, List<string>>>();

        public Dictionary<int, List<string>> WordsByLength = new Dictionary<int, List<string>>();

        public List<string> AllWords = new List<string>();

        public List<string> ItemNames = new List<string>();

        public Dictionary<char,List<string>> WordsContainingLetters = new Dictionary<char, List<string>>();

        public Dictionary<char,List<string>> WordsNotContainingLetters = new Dictionary<char, List<string>>();

        public List<string> OffsetWords = new List<string>()
        {
            "first",
            "second",
            "third",
            "fourth",
            "fifth",
            "sixth",
            "seventh",
        };

    }
}
