using OxDb.SharedCore.Client.Interfaces;
using OxDb.SharedCore.Rewards.Entities;
using System.Collections.Generic;


namespace ClientEvents
{
    public class ShowLootEvent : IClientEvent
    {
        public List<RewardList> Rewards { get; set; }
    }
}


