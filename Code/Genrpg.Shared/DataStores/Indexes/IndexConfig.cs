using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.Shared.DataStores.Indexes
{
    public class IndexConfig
    {
        public string MemberName { get; set; }
        public bool Ascending { get; set; } = true;
        public bool Unique { get; set; }  = false;
        public bool CompoundContinue { get; set; } = false;
    }
}


