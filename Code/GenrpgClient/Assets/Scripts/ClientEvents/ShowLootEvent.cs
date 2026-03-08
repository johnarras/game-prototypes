using Genrpg.Shared.Client.Interfaces;
using Genrpg.Shared.Rewards.Entities;
using Genrpg.Shared.Spawns.Entities;
using System.Collections.Generic;


namespace ClientEvents
{
    public class ShowLootEvent : IClientEvent
    {
        public List<RewardList> Rewards { get; set; }
    }
}


