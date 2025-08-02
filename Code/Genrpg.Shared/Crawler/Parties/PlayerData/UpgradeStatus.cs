using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Crawler.Parties.PlayerData
{
    // MessagePackIgnore
    public class UpgradeStatus
    {
        public long UpgradeReasonId { get; set; }
        public int RunLevel { get; set; }
        public int GameLevel { get; set; }
    }
}
