using OxDb.SharedCore.Effects.Entities;
using OxDb.SharedCore.Entities.Services;
using OxDb.SharedCore.HelperClasses;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.PlayerFiltering.Interfaces;
using OxDb.SharedGame.Effects.Helpers.DisplayHelpers;

namespace OxDb.SharedGame.Effects.Services
{
    public interface IEffectService : IInjectable
    {
        string DisplayEffect(IFilteredObject obj, IEffect effect);
        string FallbackDisplayEffect(IFilteredObject obj, IEffect effect);
    }

    public class EffectService : IEffectService
    {
        private IEntityService _entityService = null;

        private SetupDictionaryContainer<long, IEffectDisplayHelper> _displayHelpers = new SetupDictionaryContainer<long, IEffectDisplayHelper>();

        public string DisplayEffect(IFilteredObject obj, IEffect effect)
        {
            if (_displayHelpers.TryGetValue(effect.EntityTypeId, out IEffectDisplayHelper helper))
            {
                string display = helper.DisplayEffect(obj, effect);
                if (!string.IsNullOrEmpty(display))
                {
                    return display;
                }
            }

            return FallbackDisplayEffect(obj, effect);
        }

        public string FallbackDisplayEffect(IFilteredObject obj, IEffect effect)
        {
            IIdName idname = _entityService.Find(obj, effect.EntityTypeId, effect.EntityId);

            if (idname != null)
            {
                return idname.GetType().Name + " " + idname.Name + " " + effect.Quantity;
            }
            return "T/E/Q: " + effect.EntityTypeId + "/" + effect.EntityId + "/" + effect.Quantity;
        }
    }
}


