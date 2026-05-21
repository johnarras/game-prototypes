using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Names.Settings;
using System.Collections.Generic;

namespace OxDb.SharedGame.Names.Services
{

    public interface INameGenService : IInjectable
    {
        string PickWord(IRandom rand, List<WeightedName> list, string excludeName = "", string excludeWords = "");
        string PickDataListName(IRandom rand, string name);
        string PickNameListName(IRandom rand, string nameListName, string excludeName = "", string excludeWords = "");
        string PickItemName(IRandom rand, List<IIndexedGameItem> list, bool onlyShortNames = false);

        string CombinePrefixSuffix(IRandom rand, string prefix, string suffix, float hyphenChance);

        // Gen names of the following form.
        // prefix suffix.
        // If prefix is of the form "prefix of",
        // then allow suffixes of the form "the suffix",
        // otherwise don't allow suffixes of the form "the suffix"
        string GenOfTheName(IRandom rand, List<WeightedName> prefixes, List<WeightedName> suffixes, int avoidMatchingPrefixLength = 0);


        string GenerateUnitName(IRandom rand, bool forrceSuffix);
    }

}


