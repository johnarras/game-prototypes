using System;
using System.Collections.Generic;

namespace OxDb.SharedCore.DataStores.Indexes
{
    public class CreateIndexData
    {
        public Type TypeToIndex { get; private set; }
        public List<IndexConfig> Configs { get; set; } = new List<IndexConfig>();

        public CreateIndexData(Type typeToIndex)
        {
            TypeToIndex = typeToIndex;
        }
    }
}


