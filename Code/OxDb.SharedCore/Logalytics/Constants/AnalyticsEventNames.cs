using System;
using System.Collections.Generic;
using System.Text;

namespace OxDb.SharedCore.Logalytics.Constants
{
    public class AnalyticsEventNames
    {
        public const string OpenScreen = "UI.OpenScreen";
        public const string CloseScreen = "UI.CloseScreen";
        public const string ClickButton = "UI.ClickButton";


        public const string RewardInflow = "Economy.Inflow";
        public const string RewardOutflow = "Economy.Outflow";




        public const string CreateUser = "Progression.CreateUser";
        public const string GainLevel = "Progression.GainLevel";


        public const string UserSnapshot = "User.Snapshot";



        public const string FtueStartStep = "Ftue.StartStep";
        public const string FtueCompleteStep = "Ftue.CompleteStep";
        public const string CompleteStep = "Ftue.Completed";

    }
}
