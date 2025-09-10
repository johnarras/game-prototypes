using Genrpg.Shared.Effects.Helpers.DisplayHelpers;
using Genrpg.Shared.Effects.Interfaces;
using Genrpg.Shared.Entities.Services;
using Genrpg.Shared.HelperClasses;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.PlayerFiltering.Interfaces;

namespace Genrpg.Shared.Effects.Services
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
