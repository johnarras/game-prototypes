using Genrpg.Shared.Interfaces;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Awaitables
{
    public interface IAwaitableService : IInjectable
    {
        void ForgetAwaitable(Awaitable t);
        void ForgetTask(Task t);
    }

    public class AwaitableService : IAwaitableService
    {
        public void ForgetAwaitable(Awaitable t)
        {
            _ = Task.Run(async () =>
                {
                    await Awaitable.BackgroundThreadAsync();
                    await t;
                }).ConfigureAwait(false);
        }


        public void ForgetTask(Task t)
        {
            _ = Task.Run(async () =>
            {
                await Awaitable.BackgroundThreadAsync();
                await t;
            }).ConfigureAwait(false);
        }
    }
}
