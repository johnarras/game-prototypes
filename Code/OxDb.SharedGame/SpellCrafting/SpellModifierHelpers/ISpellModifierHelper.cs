using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.MapObjects.Entities;
using System.Collections.Generic;

namespace OxDb.SharedGame.SpellCrafting.SpellModifierHelpers
{
    public interface ISpellModifierHelper : ISetupDictionaryItem<long>
    {
        double GetMinValue(MapObject obj);
        double GetMaxValue(MapObject obj);
        double GetCostScale(MapObject obj, double currValue);
        double GetValidValue(MapObject obj, double currValue);
        string GetInfoText(MapObject obj);
        List<double> GetValidValues(MapObject obj);
    }
}


