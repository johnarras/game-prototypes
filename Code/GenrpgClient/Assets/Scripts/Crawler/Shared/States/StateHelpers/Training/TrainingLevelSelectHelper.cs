using OxDb.SharedGame.Crawler.States.Constants;

namespace OxDb.Client.Crawler.Shared.States.StateHelpers.Training
{
    public class TrainingLevelSelectHelper : BaseTrainingSelectMemberHelper
    {
        public override ECrawlerStates HelperKey => ECrawlerStates.TrainingLevelSelect;

        public override string GetMainMessage() { return "Which party member will level up?"; }

        public override ECrawlerStates GetNextState() { return ECrawlerStates.TrainingLevelMember; }
    }
}


