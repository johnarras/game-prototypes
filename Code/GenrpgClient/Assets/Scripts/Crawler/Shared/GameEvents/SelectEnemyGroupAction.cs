using OxDb.SharedCore.Client.Interfaces;
using OxDb.SharedGame.Crawler.Combat.Entities;
using System;

namespace OxDb.SharedGame.Crawler.GameEvents
{
    public class SetSelectEnemyGroupAction : IClientEvent
    {
        public Action Action;
        public CombatGroup Group;
    }
}


