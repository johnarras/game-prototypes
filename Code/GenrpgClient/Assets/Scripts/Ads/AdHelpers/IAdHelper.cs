using OxDb.SharedCore.Interfaces;
using UnityEngine;

namespace OxDb.Client.Ads.AdHelpers
{

    public enum EAdTypes
    {
        Rewarded = 1,
    };

    public class AdResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class AdArgs
    {
        public string AdUnitId { get; set; }
    }

    public interface IAdHelper : ISetupDictionaryItem<EAdTypes>
    {
        Awaitable<AdResult> ShowAd(AdArgs args = null);
    }
}
