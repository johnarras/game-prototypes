using OxDb.SharedCore.Interfaces;
using System.Threading.Tasks;

namespace OxDb.SharedCore.Tasks.Services
{
    public interface ITaskService : IInjectable
    {
        void ForgetTask(Task t, bool isLongRunning);
    }

    public class TaskService : ITaskService
    {
        public void ForgetTask(Task t, bool isLongRunning)
        {
            if (!isLongRunning)
            {
                _ = Task.Run(() => t).ConfigureAwait(false);
            }
            else
            {
               Task.Factory.StartNew(() => t, TaskCreationOptions.LongRunning).ConfigureAwait(false);
            }
        }
    }
}


