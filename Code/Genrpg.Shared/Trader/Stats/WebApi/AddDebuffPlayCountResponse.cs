using Genrpg.Shared.Website.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Trader.Stats.WebApi
{
    public class AddDebuffPlayCountResponse : IWebResponse
    {
        public int DebuffDaysAdded { get; set; }
    }
}
