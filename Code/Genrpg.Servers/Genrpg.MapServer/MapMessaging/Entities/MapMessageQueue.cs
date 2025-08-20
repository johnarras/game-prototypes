using Genrpg.MapServer.MapMessaging.Interfaces;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.MapMessages.Interfaces;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Tasks.Services;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.MapServer.MapMessaging.Entities
{
    public class MapMessageQueue
    {
        const int DelayedMessageBufferSize = 10000;

        protected ConcurrentQueue<MapMessagePackage> _currentQueue = new ConcurrentQueue<MapMessagePackage>();
        protected ConcurrentQueue<MapMessagePackage> _delayedQueue = new ConcurrentQueue<MapMessagePackage>();
        protected List<MapMessagePackage>[] _delayedMessages = null;
        protected int _tick = 0;
        protected int _queueIndex = -1;
        protected long _messagesProcessed = 0;
        protected CancellationToken _token;
        private ILogService _logService;
        private IMapMessageService _mapMessageService = null;
        private IGameData _gameData = null;
        private ITaskService _taskService = null;

        private DateTime _startTime = DateTime.UtcNow;

        public MapMessageQueue(DateTime startTime, int queueIndex, ILogService logService, IMapMessageService mapMessageService, ITaskService taskService, CancellationToken token)
        {
            _token = token;
            _logService = logService;
            _startTime = startTime;
            _taskService = taskService;
            _queueIndex = queueIndex;
            _mapMessageService = mapMessageService;
            _delayedMessages = new List<MapMessagePackage>[DelayedMessageBufferSize];
            for (int d = 0; d < _delayedMessages.Length; d++)
            {
                _delayedMessages[d] = new List<MapMessagePackage>();
            }

            _taskService.ForgetTask(ProcessDelayQueue(_token), true);
            _taskService.ForgetTask(ProcessQueue(_token), true);
        }

        public long GetMessagesProcessed()
        {
            return _messagesProcessed;
        }

        public void UpdateGameData(IGameData gameData)
        {
            _gameData.CopyFrom(gameData);
            _logService.Message("Update Message Queue Game Data!");
        }

        public void AddMessage(IMapMessage message, MapObject mapObject, IMapMessageHandler handler, float delaySeconds)
        {
            MapMessagePackage package = mapObject.TakePackage();

            package.message = message;
            package.mapObject = mapObject;
            package.handler = handler;
            package.delaySeconds = delaySeconds;

            if (package.delaySeconds <= 0)
            {
                package.message.SetLastExecuteTime(DateTime.UtcNow);
                _currentQueue.Enqueue(package);
            }
            else
            {
                _delayedQueue.Enqueue(package);
            }
        }

        protected async Task ProcessDelayQueue(CancellationToken token)
        {
            try
            {
                int currentTick = 0;

                using (PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromSeconds(MessageConstants.DelayedMessageTimeGranularity)))
                {
                    await timer.WaitForNextTickAsync(_token).ConfigureAwait(false);

                    while (true)
                    {
                        DateTime tickStartTime = DateTime.UtcNow;
                        while (_delayedQueue.TryDequeue(out MapMessagePackage item))
                        {
                            DateTime nextExecuteTime = item.message.GetLastExecuteTime().AddSeconds(item.delaySeconds);
                            double messageTimeDiff = Math.Max(0, (nextExecuteTime - DateTime.UtcNow).TotalSeconds);
                            int messageTimeTicks = (int)(messageTimeDiff /= MessageConstants.DelayedMessageTimeGranularity);
                            int offset = MathUtils.Clamp(1, messageTimeTicks, DelayedMessageBufferSize - 1);
                            int index = (currentTick + offset) % DelayedMessageBufferSize;
                            item.message.SetLastExecuteTime(nextExecuteTime);
                            _delayedMessages[index].Add(item);
                        }

                        // Find the new tick based on the time elapsed from start of game.
                        int newTick = (int)((DateTime.UtcNow - _startTime).TotalSeconds / MessageConstants.DelayedMessageTimeGranularity);

                        for (int i = currentTick + 1; i <= newTick; i++)
                        {
                            // For each tick in between currentTick and newTick, pull all messages into the main queue.
                            int idx = i % DelayedMessageBufferSize;
                            List<MapMessagePackage> newMessages = _delayedMessages[idx];
                            _delayedMessages[idx] = new List<MapMessagePackage>();
                            foreach (MapMessagePackage package in newMessages)
                            {
                                _currentQueue.Enqueue(package);
                            }
                        }
                        // Update the current tick as far as we need to go.
                        currentTick = newTick;

                        await timer.WaitForNextTickAsync(_token).ConfigureAwait(false);
                    }
                }
            }
            catch (OperationCanceledException oce)
            {
                _logService.Info("Map Instance Shutdown MessageQueue.ProcessDelayed Index: " + oce.Message + " " + _queueIndex);
            }
            catch (Exception e)
            {
                _logService.Exception(e, "MessageQueueDelay");
            }
        }
        protected async Task ProcessQueue(CancellationToken token)
        {
            try
            {
                MyRandom rand = new MyRandom();
                using (PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromMilliseconds(1)))
                {
                    await timer.WaitForNextTickAsync(_token).ConfigureAwait(false);
                    while (true)
                    {
                        while (_currentQueue.TryDequeue(out MapMessagePackage package))
                        {
                            try
                            {
                                // This is intentionally synchronous
                                package.Process(rand);
                                _messagesProcessed++;
                                package.mapObject.ReturnPackage(package);
                            }
                            catch (Exception e)
                            {
                                _logService.Exception(e, "Process Message");
                            }

                            if (_messagesProcessed % 100 == 0)
                            {
                                await Task.CompletedTask;
                            }
                        }
                        await timer.WaitForNextTickAsync(_token).ConfigureAwait(false);
                    }
                }
            }
            catch (OperationCanceledException oce)
            {
                _logService.Info("Map Instance Shutdown MessageQueue.Process Index: " + oce.Message + " " + _queueIndex);
            }
            catch (Exception e)
            {
                _logService.Exception(e, "MessageQueueProcess");
            }
        }
    }
}
