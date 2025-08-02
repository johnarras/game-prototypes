using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Genrpg.Shared.Utils
{
    public static class CollectionUtils
    {

        public static long GetNextIdKey<T>(IEnumerable<T> list, long defaultValue = 0) where T : IId
        {
            return list.Select(x => x.IdKey).DefaultIfEmpty(defaultValue).Max() + 1;
        }
    }
}
