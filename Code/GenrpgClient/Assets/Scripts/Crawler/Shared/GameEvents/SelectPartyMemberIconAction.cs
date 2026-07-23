using OxDb.SharedCore.Client.Interfaces;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using System;

namespace OxDb.Client.Crawler.Shared.GameEvents
{
    public class SelectPartyMemberIconAction : IClientEvent
    {
        public PartyMember Member;
        public Action ClickAction;
    }
}
