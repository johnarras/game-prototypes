using Genrpg.Shared.Interfaces;
using Genrpg.Shared.PlayerFiltering.Settings;
using System;
using System.Collections.Generic;

namespace Genrpg.Shared.PlayerFiltering.Interfaces
{
    public interface IPlayerFilter : IIdName
    {
        bool Enabled { get; set; }
        long TotalModSize { get; set; }
        long MaxModValue { get; set; }
        long Priority { get; set; }

        long MinLevel { get; set; }
        long MaxLevel { get; set; }
        double MinInstallDays { get; set; }
        double MaxInstallDays { get; set; }

        long MinPurchaseCount { get; set; }
        long MaxPurchaseCount { get; set; }
        double MinPurchaseTotal { get; set; }
        double MaxPurchaseTotal { get; set; }

        DateTime StartDate { get; set; }
        DateTime EndDate { get; set; }
        int RepeatHours { get; set; }
        bool RepeatMonthly { get; set; }

        string MinClientVersion { get; set; }
        string MaxClientVersion { get; set; }

        void OrderSelf();
        List<AllowedPlayer> AllowedPlayers { get; set; }
    }
}
