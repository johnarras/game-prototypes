using OxDb.SharedGame.Crawler.States.Constants;

namespace OxDb.Client.Crawler.Shared.States.StateHelpers.Training
{
    public class TrainingUpgradeSelectHelper : BaseTrainingSelectMemberHelper
    {
        public override ECrawlerStates HelperKey => ECrawlerStates.TrainingUpgradeSelect;

        public override string GetMainMessage() { return "Which party member will get some upgrades?"; }

        public override ECrawlerStates GetNextState() { return ECrawlerStates.TrainingUpgradeMember; }
    }
}


