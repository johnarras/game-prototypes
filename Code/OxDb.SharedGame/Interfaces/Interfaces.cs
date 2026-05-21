using OxDb.SharedCore.DataStores.Interfaces;
using OxDb.SharedCore.Effects.Entities;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.DataStores.Categories.PlayerData.ParentChild;
using System;
using System.Collections.Generic;

namespace OxDb.SharedGame.Interfaces
{

    public interface IRealtimeGame : IStringId
    {
    }

    public interface IStringOwnerId : ISearchableItem
    {
        string OwnerId { get; set; }
    }

    public interface IMapOwnerId : IStringOwnerId
    {
        string MapId { get; set; }
    }

    public interface IOwnerQuantityChild : IStringOwnerId, IChildUnitData, IId
    {
        long Quantity { get; set; }
    }

    public interface INamedUpdateData : IName, IVersionedData
    {

    }

    public interface IEffectList<T> where T : class, IEffect
    {
        List<T> Effects { get; set; }
    }

    public interface IMusicRegion
    {
        long MusicTypeId { get; set; }
        long AmbientMusicTypeId { get; set; }
    }



    public interface ISpellHit
    {
        string UnitId { get; set; }
        DateTime LastHitTime { get; set; }
        int NumHits { get; set; }
    }


}
