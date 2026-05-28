using OxDb.SharedCore.Core.Constants;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Logalytics.Constants;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedCore.Utils.Data;
using OxDb.SharedGame.Characters.PlayerData;
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
        Task<bool> IsComplete(IUnitDataLookup lookup);
        Task<FtueStep> GetCurrentStep(IUnitDataLookup lookup);
        Task<FtueStep> StartNextStep(IUnitDataLookup lookup);
        Task<bool> CanClickButton(IUnitDataLookup lookup, string screenName, string buttonName);
        Task<FtueStep> CompleteStep(IUnitDataLookup lookup, long ftueStepId, IRandom rand);
        Task<FtueStep> ForceStartStep(IUnitDataLookup lookup, long ftueStepId);
    }
    public class FtueService : IFtueService
    {

        private IGameData _gameData = null;

        public async Task<FtueStep> GetCurrentStep(IUnitDataLookup lookup)
        {
            CoreData coreData = await lookup.GetAsync<CoreData>();

            if (coreData.HasFlag(TraderFlags.CompletedFtue))
            {
                return null;
            }

            FtueData ftueData = await lookup.GetAsync<FtueData>();

            return _gameData.Get<FtueStepSettings>(coreData).Get(ftueData.CurrentFtueStepId);
        }

        public async Task<bool> CanClickButton(IUnitDataLookup lookup, string screenName, string buttonName)
        {
            CoreData coreData = await lookup.GetAsync<CoreData>();

            if (coreData.HasFlag(TraderFlags.CompletedFtue))
            {
                return true;
            }

            FtueStep step = await GetCurrentStep(lookup);

            if (step != null && string.Compare(screenName, step.ActionScreenName, true) == 0
                && string.Compare(buttonName, step.ActionButtonName, true) == 0)
            {
                return true;
            }


            return false;
        }

        public async Task<FtueStep> CompleteStep(IUnitDataLookup lookup, long ftueStepId, IRandom rand)
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

        public virtual async Task<FtueStep> StartNextStep(IUnitDataLookup lookup)
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

        public async Task<bool> IsComplete(IUnitDataLookup lookup)
        {
            



            CoreData coreData = await lookup.GetAsync<CoreData>();

            return coreData.HasFlag(TraderFlags.CompletedFtue);
        }
        
        protected virtual void ShowAnalytics(string analyticsEventName, FtueStep step)
        {

        }

        public async Task<FtueStep> SetCurrentStep(IUnitDataLookup lookup, long ftueStepId)
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

        public async Task<FtueStep> ForceStartStep(IUnitDataLookup lookup, long ftueStepId)
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


