using OxDb.SharedCore.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace OxDb.SharedCore.HelperClasses
{
    public class OrderedSetupDictionaryContainer<Key, Val> : SetupDictionaryContainer<Key, Val>, IInitOnResolve where Val : IOrderedSetupDictionaryItem<Key>
    {
        public IEnumerable<Val> OrderedItems() { return _dictionary.Values.OrderBy(x => x.Order); }
    }
}


