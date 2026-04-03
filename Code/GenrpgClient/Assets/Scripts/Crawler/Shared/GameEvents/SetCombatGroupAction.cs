using Genrpg.Shared.Client.Interfaces;
using Genrpg.Shared.Crawler.Combat.Entities;
using System;

namespace Genrpg.Shared.Crawler.GameEvents
{
    public class SetCombatGroupAction : IClientEvent
    {
        public Action Action;
        public CombatGroup Group;
    }
}


