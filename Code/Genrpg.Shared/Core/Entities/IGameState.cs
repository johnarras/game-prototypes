using Genrpg.Shared.Interfaces;

namespace Genrpg.Shared.Core.Entities
{
    // Encapsulate all parameters necessary to call a command.


    public interface IGameState
    {
        IServiceLocator loc { get; }
    }

    public class GameState : IGameState
    {
        // Shared data
        public IServiceLocator loc => _loc;
        /// <summary>
        /// This is here to make sure that within any specific thread/task the random number generator is not shared
        /// where it might erroring out. And there's got to be overhead with Random.Shared [ThreadStatic] so it's easier to 
        /// explicitly manage this per thread/task. With 100 tasks, Shared.Random is about half as fast. Idk if that matters but
        /// I can also set seeds for each thread too.
        /// </summary>


        protected IServiceLocator _loc = null;

        public GameState()
        {
        }

    }
}


