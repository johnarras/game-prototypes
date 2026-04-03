using Genrpg.Shared.Crawler.States.Constants;

namespace Assets.Scripts.Crawler.Shared.States.StateHelpers.Training
{
    public class TrainingUpgradeSelectHelper : BaseTrainingSelectMemberHelper
    {
        public override ECrawlerStates HelperKey => ECrawlerStates.TrainingUpgradeSelect;

        public override string GetMainMessage() { return "Which party member will get some upgrades?"; }

        public override ECrawlerStates GetNextState() { return ECrawlerStates.TrainingUpgradeMember; }
    }
}


