using OxDb.SharedCore.Client.Interfaces;
using OxDb.SharedCore.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public delegate void GameAction<T>(T t);

public interface IDispatcher : IInitializable
{
    void AddListener<T>(GameAction<T> action, CancellationToken token) where T : class, IClientEvent;
    void Dispatch<T>(T actionParam) where T : class, IClientEvent;

}

public class Dispatcher : IDispatcher
{
    public async Task Initialize(CancellationToken token)
    {
        await Task.CompletedTask;
    }

    private Dictionary<Type, object> _dict = new Dictionary<Type, object>();

    public void AddListener<T>(GameAction<T> action, CancellationToken token) where T : class, IClientEvent
    {
        token.Register(() =>
        {
            RemoveListener(action);
        });

        if (!_dict.ContainsKey(typeof(T)))
        {
            _dict[typeof(T)] = new List<GameAction<T>>();
        }

        List<GameAction<T>> list = (List<GameAction<T>>)_dict[typeof(T)];
        if (!list.Any(x => x.Target == action.Target))
        {
            list.Add(action);
        }
    }

    /// <summary>
    /// May need to make this public someday, but these events seem to stick around for the life of the object.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="action"></param>
    private void RemoveListener<T>(GameAction<T> action) where T : class, IClientEvent
    {
        if (!_dict.ContainsKey(typeof(T)))
        {
            return;
        }
        List<GameAction<T>> list = (List<GameAction<T>>)_dict[typeof(T)];
        if (list.Contains(action))
        {
            list.Remove(action);
        }
    }

    public void Dispatch<T>(T actionParam) where T : class, IClientEvent
    {
        if (!_dict.ContainsKey(typeof(T)))
        {
            return;
        }

        List<GameAction<T>> list = new List<GameAction<T>>((List<GameAction<T>>)_dict[typeof(T)]);

        foreach (GameAction<T> gameAction in list)
        {
            gameAction(actionParam);
        }
    }
}


