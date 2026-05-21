using OxDb.MapServer.MapMessaging.Interfaces;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.MapMessages.Interfaces;
using OxDb.SharedGame.MapObjects.Entities;
using OxDb.SharedGame.MapServer.Entities;
using OxDb.SharedGame.Tasks.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.MapServer.MapMessaging.Entities
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

        protected MyRandom _rand = new MyRandom();

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
            _logService.Info("Update Message Queue Game Data!");
        }

        private ConcurrentQueue<MapMessagePackage> _packagePool = new ConcurrentQueue<MapMessagePackage>();
        protected MapMessagePackage CheckoutPackage()
        {
            if (_packagePool.TryDequeue(out MapMessagePackage pack))
            {
                return pack;
            }
            return new MapMessagePackage();
        }

        protected void ReturnPackage(MapMessagePackage pack)
        {
            pack.Clear();
            _packagePool.Enqueue(pack);
        }

        public void AddMessage(IMapMessage message, MapObject mapObject, IMapMessageHandler handler, float delaySeconds)
        {
            MapMessagePackage package = CheckoutPackage();

            package.Message = message;
            package.MapObject = mapObject;
            package.Handler = handler;
            package.delaySeconds = delaySeconds;
            package.Rand = _rand;

            if (package.delaySeconds <= 0)
            {
                package.Message.SetLastExecuteTime(DateTime.UtcNow);
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
                            DateTime nextExecuteTime = item.Message.GetLastExecuteTime().AddSeconds(item.delaySeconds);
                            double messageTimeDiff = Math.Max(0, (nextExecuteTime - DateTime.UtcNow).TotalSeconds);
                            int messageTimeTicks = (int)(messageTimeDiff /= MessageConstants.DelayedMessageTimeGranularity);
                            int offset = MathUtil.Clamp(1, messageTimeTicks, DelayedMessageBufferSize - 1);
                            int index = (currentTick + offset) % DelayedMessageBufferSize;
                            item.Message.SetLastExecuteTime(nextExecuteTime);
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
                using (PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromMilliseconds(1)))
                {
                    await timer.WaitForNextTickAsync(_token).ConfigureAwait(false);
                    while (true)
                    {
                        while (_currentQueue.TryDequeue(out MapMessagePackage package))
                        {
                            try
                            {
                                await package.Handler.Process(package);
                                _messagesProcessed++;
                                ReturnPackage(package);
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


