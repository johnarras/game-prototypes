using OxDb.SharedGame.Crawler.States.Constants;

namespace Assets.Scripts.Crawler.Shared.States.StateHelpers.Training
{
    public class TrainingLevelSelectHelper : BaseTrainingSelectMemberHelper
    {
        public override ECrawlerStates HelperKey => ECrawlerStates.TrainingLevelSelect;

        public override string GetMainMessage() { return "Which party member will level up?"; }

        public override ECrawlerStates GetNextState() { return ECrawlerStates.TrainingLevelMember; }
    }
}


