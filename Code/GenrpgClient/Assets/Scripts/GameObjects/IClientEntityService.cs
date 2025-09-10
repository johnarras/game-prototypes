using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;

public interface IClientEntityService : IInjectable
{
    object FullInstantiateAndSet(object obj);
    C FullInstantiate<C>(C c) where C : class;
    object FullInstantiate(object obj);
    void InitializeHierarchy(object obj);
    C GetOrAddComponent<C>(object obj) where C : class;
    void SetActive(object obj, bool value);
    void Destroy(object obj);
    void WaitToDestroy(object obj, float waitSeconds, CancellationToken token);
    object FindChild(object objIn, string name);
    List<T> GetComponents<T>(object obj);
    T GetInterface<T>(object obj);
    T GetComponent<T>(object obj) where T : class;
    T FindInParents<T>(object obj) where T : class;
    void DestroyAllChildren(object obj);
    void SetLayer(object obj, string layerName);
    void SetLayer(object obj, int layer);
    void AddToParent(object childObj, object parentObj);
    object GetEntity(object obj);
    void RegisterDestroyCallback(object obj, Action action);

}
