using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils;
using MessagePack.Formatters;
using System.Collections.Generic;

namespace Genrpg.Shared.HelperClasses
{
    public class SetupDictionaryContainer<Key, Val> : IInitOnResolve where Val : ISetupDictionaryItem<Key>
    {
        protected IServiceLocator _loc = null!;
        protected IReflectionService _reflectionService = null;
        protected Dictionary<Key, Val> _dictionary = new Dictionary<Key, Val>();
        public void Init()
        {
            _dictionary = _reflectionService.SetupDictionary<Key, Val>(_loc);
        }

        public bool TryGetValue(Key key, out Val value)
        {
            if (_dictionary.TryGetValue(key, out value))
            {
                return true;
            }
            return false;
        }

        public Dictionary<Key, Val> GetDict()
        {
            return _dictionary;
        }
    }
}


