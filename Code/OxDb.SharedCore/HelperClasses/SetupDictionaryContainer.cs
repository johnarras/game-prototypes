using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Utils;
using System.Collections.Generic;

namespace OxDb.SharedCore.HelperClasses
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


