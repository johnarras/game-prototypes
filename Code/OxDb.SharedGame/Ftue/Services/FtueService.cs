using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Logalytics.Constants;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Core.PlayerData;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;
using OxDb.SharedGame.Ftue.PlayerData;
using OxDb.SharedGame.Ftue.Settings.Steps;
using OxDb.SharedGame.Trader.Flags.Constants;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OxDb.SharedGame.Ftue.Services
{
    public interface IFtueService : IInjectable
    {
        ValueTask<bool> IsComplete(IUnitDataLookup lookup);
        ValueTask<FtueStep> GetCurrentStep(IUnitDataLookup lookup);
        ValueTask<FtueStep> StartNextStep(IUnitDataLookup lookup);
        ValueTask<bool> CanClickButton(IUnitDataLookup lookup, string screenName, string buttonName);
        ValueTask<FtueStep> CompleteStep(IUnitDataLookup lookup, long ftueStepId);
        ValueTask<FtueStep> ForceStartStep(IUnitDataLookup lookup, long ftueStepId);
    }
    public class FtueService : IFtueService
    {
        private IGameData _gameData = null;

        public async ValueTask<FtueStep> GetCurrentStep(IUnitDataLookup lookup)
        {
            CoreData coreData = await lookup.GetAsync<CoreData>();

            if (coreData.HasFlag(TraderFlags.CompletedFtue))
            {
                return null;
            }

            FtueData ftueData = await lookup.GetAsync<FtueData>();

            return _gameData.Get<FtueStepSettings>(coreData).Get(ftueData.CurrentFtueStepId);
        }

        public async ValueTask<bool> CanClickButton(IUnitDataLookup lookup, string screenName, string buttonName)
        {

            if (await IsComplete(lookup))
            {
                return true;
            }

            FtueStep step = await GetCurrentStep(lookup);

            if (step != null && !string.IsNullOrEmpty(step.ActionScreenName) && !string.IsNullOrEmpty(step.ActionButtonName))
            {
                if (string.Compare(screenName, step.ActionScreenName, true) == 0
                    && string.Compare(buttonName, step.ActionButtonName, true) == 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        public async ValueTask<FtueStep> CompleteStep(IUnitDataLookup lookup, long ftueStepId)
        {

            FtueStep ftueStep = await GetCurrentStep(lookup);

            if (ftueStep == null || ftueStep.IdKey != ftueStepId)
            {
                return null;
            }

            CoreData coreData = await lookup.GetAsync<CoreData>();

            if (coreData.HasFlag(TraderFlags.CompletedFtue))
            {
                return null;
            }

            FtueData ftueData = await lookup.GetAsync<FtueData>();

            if (ftueData.CurrentFtueStepId != ftueStepId)
            {
                return null;
            }

            if (!ftueData.CompletedFtues.HasBitIndex(ftueStepId))
            {
                ftueData.CompletedFtues.SetBitIndex(ftueStepId);
                ShowAnalytics(AnalyticsEventNames.FtueCompleteStep, ftueStep);
            }

            ftueData.PrevFtueStepId = ftueData.CurrentFtueStepId;
            ftueData.CurrentFtueStepId = 0;
            return await StartNextStep(lookup);

        }

        public virtual async ValueTask<FtueStep> StartNextStep(IUnitDataLookup lookup)
        {
            FtueData ftueData = await lookup.GetAsync<FtueData>();

            CoreData coreData = await lookup.GetAsync<CoreData>();

            long currentStepId = ftueData.CurrentFtueStepId;

            if (ftueData.CurrentFtueStepId > 0)
            {
                FtueStep ftueStep = _gameData.Get<FtueStepSettings>(coreData).Get(ftueData.CurrentFtueStepId);

                if (ftueStep != null)
                {
                    return ftueStep;
                }
                else
                {
                    ftueData.CurrentFtueStepId = 0;
                }
            }

            List<FtueStep> nextSteps = _gameData.Get<FtueStepSettings>(coreData).GetData().Where(x => x.PrereqFtueStepId == ftueData.PrevFtueStepId).ToList();

            if (nextSteps.Count > 0)
            {
                if (currentStepId > 0)
                {
                    ftueData.PrevFtueStepId = currentStepId;
                }
                return await SetCurrentStep(lookup, currentStepId);
            }
            else
            {
                coreData.AddFlag(TraderFlags.CompletedFtue);
            }

            return null;
        }

        public async ValueTask<bool> IsComplete(IUnitDataLookup lookup)
        {
            if (lookup == null)
            {
                return true;
            }

            CoreData coreData = await lookup.GetAsync<CoreData>();

            if (coreData == null)
            {
                return true;
            }

            return coreData.HasFlag(TraderFlags.CompletedFtue);
        }

        protected virtual void ShowAnalytics(string analyticsEventName, FtueStep step)
        {

        }

        public async ValueTask<FtueStep> SetCurrentStep(IUnitDataLookup lookup, long ftueStepId)
        {

            if (await IsComplete(lookup))
            {
                return null;
            }

            FtueData ftueData = await lookup.GetAsync<FtueData>();

            if (ftueData.CurrentFtueStepId == ftueStepId || ftueData.CompletedFtues.HasBitIndex(ftueStepId))
            {
                return null;
            }

            CoreData coreData = await lookup.GetAsync<CoreData>();

            FtueStep currentFtueStep = _gameData.Get<FtueStepSettings>(coreData).Get(ftueData.CurrentFtueStepId);

            FtueStep nextFtueStep = _gameData.Get<FtueStepSettings>(coreData).Get(ftueStepId);

            if (nextFtueStep == null)
            {
                return null;
            }

            if (ftueData.CurrentFtueStepId > 0)
            {
                ftueData.PrevFtueStepId = ftueData.CurrentFtueStepId;
            }

            ftueData.CurrentFtueStepId = ftueStepId;
            ShowAnalytics(AnalyticsEventNames.FtueStartStep, currentFtueStep);

            return currentFtueStep;
        }

        public async ValueTask<FtueStep> ForceStartStep(IUnitDataLookup lookup, long ftueStepId)
        {
            FtueStep currStep = await GetCurrentStep(lookup);

            if (currStep != null)
            {
                return currStep;
            }

            CoreData coreData = await lookup.GetAsync<CoreData>();

            FtueStep nextStep = _gameData.Get<FtueStepSettings>(coreData).Get(ftueStepId);

            if (nextStep != null)
            {
                return await SetCurrentStep(lookup, ftueStepId);
            }

            return null;
        }
    }
}


