
using Genrpg.Shared.DataStores.Categories.PlayerData.ParentChild;
using Genrpg.Shared.DataStores.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Genrpg.Shared.Interfaces
{

    public delegate void VoidDelegate();
    public delegate void ObjectDelegate(object obj);
    public delegate void StringDelegate(string s);
    public delegate void TokenDelegate(CancellationToken token);


    public interface IRealtimeGame : IStringId
    {
    }

    public interface IStringId
    {
        string Id { get; set; }
    }

    public interface IStringOwnerId : IStringId
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

    public interface IId
    {
        long IdKey { get; set; }
    }

    public interface IDbId
    {
        long Id { get; set; }
    }
    public interface IName
    {
        string Name { get; set; }
    }


    public interface INamedUpdateData : IName, IUpdateData
    {

    }

    public interface IIdName : IId, IName
    {

    }

    public interface IIndexedGameItem : IIdName
    {
        string Desc { get; set; }
        string AtlasPrefix { get; set; }
        string Icon { get; set; }
        string Art { get; set; }
    }

    public interface IOrderedItem
    {
        long GetOrder();
    }

    public interface IVariationIndexedGameItem : IIndexedGameItem
    {
        int VariationCount { get; set; }
    }

    public interface INameId : IIdName
    {
        string NameId { get; set; }
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


    public interface IServiceLocator
    {
        T Get<T>() where T : IInjectable;
        void Set<T>(T t) where T : IInjectable;
        void SetExplicitType(Type interfaceType, object obj);
        List<Type> GetKeys();

        List<T> GetVals<T>();
        void Resolve(object obj);
        void StoreDictionaryItem(object obj);
        void ResolveSelf();
    }
}


