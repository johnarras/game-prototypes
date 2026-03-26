using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Website.Interfaces;
using System.Collections.Generic;

namespace Genrpg.Shared.LevelTracks.WebApi
{
    public class GainExpResponse : IWebResponse
    {
        public List<LevelGained> LevelsGained { get; set; } = new List<LevelGained>();

        public long StartLevel { get; set; }
        public long StartExp { get; set; }

        public long ExpGained { get; set; }

        public long StartExpToLevelUp { get; set; }
        public long EndLevel { get; set; }
        public long EndExp { get; set; }

        public long EndExpToLevel { get; set; }
    }

    public class LevelGained
    {
        public long NewLevel { get; set; }
        public List<Reward> Rewards { get; set; } = new List<Reward>();
    }
}
