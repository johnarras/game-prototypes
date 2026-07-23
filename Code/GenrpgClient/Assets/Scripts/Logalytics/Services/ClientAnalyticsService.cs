using OxDb.Client.Core.Interfaces;
using OxDb.Client.Logalytics.Utils;
using OxDb.Client.Networking.Services;
using OxDb.SharedCore.Core.Constants;
using OxDb.SharedCore.Entities.Services;
using OxDb.SharedCore.Entities.Settings;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Logalytics.Constants;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Rewards.Entities;
using OxDb.SharedCore.Serialization.Interfaces;
using OxDb.SharedCore.Setup.Constants;
using OxDb.SharedCore.Utils;
using OxDb.SharedCore.WebRequests.Services;
using OxDb.SharedGame.Rewards.Settings;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace OxDb.Client.Logalytics.Services
{
    #region Telemetry JSON Schema Structures

    [Serializable]
    public struct TelemetryEnvelope
    {
        public string name { get; set; }
        public string time { get; set; }
        public string iKey { get; set; }
        public int ver { get; set; }
        public TelemetryData data { get; set; }
    }

    [Serializable]
    public struct TelemetryData
    {
        public string baseType { get; set; }
        public EventData baseData { get; set; }
    }

    [Serializable]
    public struct EventData
    {
        public int ver { get; set; }
        public string name { get; set; }
        public Dictionary<string, string> properties { get; set; }
        public Dictionary<string, double> measurements { get; set; }
    }

    #endregion

    public class ClientAnalyticsService : IAnalyticsService, IClientQuitCleanup, IClientResetCleanup
    {
        protected IClientConfigContainer _configContainer = null;
        private ITextSerializer _textSerializer = null;
        private IClientUpdateService _updateService = null;
        private IClientGameState _gs = null;
        private ILogService _logService = null;
        private IGameData _gameData = null;
        private IEntityService _entityService = null;
        private IClientLogalyticsService _logalyticsService = null;
        private IClientWebRequestService _webRequestService = null;

        protected string IngestionEndpoint { get; set; }
        protected string InstrumentationKey { get; set; }
        protected ConcurrentQueue<TelemetryEnvelope> TelemetryQueue { get; set; } = new ConcurrentQueue<TelemetryEnvelope>();
        protected bool IsProcessingQueue { get; set; }

        // Batch throttling optimization parameters
        private const int MaxBatchSize = 50;
        private const float MaxBatchDelaySeconds = 10.0f;
        DateTime _lastSendTime = DateTime.UtcNow;

        protected bool _didInitialize = false;

        public async Task Initialize(CancellationToken token)
        {
            if (GameModeUtils.IsPureClientMode(_gs.GameMode))
            {
                return;
            }

            string connectionString = LogalyticsUtils.GetAnalyticsConnectionString(_configContainer.Config);

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                _didInitialize = false;
                UnityEngine.Debug.LogError("Missing App Insights Connection String inside Logging Initialization");
                return;
            }

            string[] parts = connectionString.Split(';');
            foreach (string part in parts)
            {
                if (part.StartsWith("InstrumentationKey=", StringComparison.OrdinalIgnoreCase))
                {
                    InstrumentationKey = part.Substring("InstrumentationKey=".Length);
                }
                else if (part.StartsWith("IngestionEndpoint=", StringComparison.OrdinalIgnoreCase))
                {
                    string endpoint = part.Substring("IngestionEndpoint=".Length).TrimEnd('/');
                    //IngestionEndpoint = $"{endpoint}";
                    IngestionEndpoint = $"{endpoint}/v2.1/track";
                }
            }

            if (string.IsNullOrEmpty(InstrumentationKey) || string.IsNullOrEmpty(IngestionEndpoint))
            {
                Debug.LogError("Failed to parse infrastructure parameters for logging runtime configuration.");
                return;
            }

            _didInitialize = true;

            _updateService.AddUpdate(this, UpdateService, UpdateTypes.Late, token);
            await Task.CompletedTask;
        }

        public void TrackEvent(string eventName, Dictionary<string, string> properties = null, Dictionary<string, double> measurements = null)
        {
            // Fixed inverse logic initialization check
            if (!_didInitialize)
            {
                return;
            }

            if (properties == null)
            {
                properties = new Dictionary<string, string>();

            }

            Dictionary<string, string> defaultDimensions = _logalyticsService.GetDefaultLogalyticsDimensions();

            foreach (string key in defaultDimensions.Keys)
            {
                properties[key] = defaultDimensions[key];
            }

            if (measurements == null)
            {
                measurements = new Dictionary<string, double>();
            }

            measurements[AnalyticsKeys.SessionSequenceId] = ++_gs.SessionSequenceId;

            TelemetryEnvelope envelope = new TelemetryEnvelope
            {
                name = "Event",
                time = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.ffffffZ"),
                ver = 1,
                iKey = InstrumentationKey,
                data = new TelemetryData
                {
                    baseType = "EventData",
                    baseData = new EventData
                    {
                        ver = 2,
                        name = eventName,
                        properties = properties ?? new Dictionary<string, string>(),
                        measurements = measurements ?? new Dictionary<string, double>(),
                    }
                }
            };

            TelemetryQueue.Enqueue(envelope);

            // Fast-track kick-off if a massive event spike completely fills a batch limit instantly
            if (!IsProcessingQueue && TelemetryQueue.Count >= MaxBatchSize)
            {
                ProcessQueue();
            }
        }

        protected void AddNonNullValue(Dictionary<string, string> dict, string key, string val)
        {
            if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(val))
            {
                dict[key] = val;
            }
        }

        public void TrackUIEvent(string eventName, string screenName, string buttonName = null, Dictionary<string, string> properties = null, Dictionary<string, double> measurements = null)
        {

            if (properties == null)
            {
                properties = new Dictionary<string, string>();
            }

            AddNonNullValue(properties, AnalyticsKeys.ScreenName, StrUtils.ToSnakeCase(screenName));
            AddNonNullValue(properties, AnalyticsKeys.ButtonName, StrUtils.ToSnakeCase(buttonName));

            TrackEvent(eventName, properties, measurements);
        }

        public void TrackEconomyEvent(string eventName, long entityTypeId, long entityId, long quantity, long rewardSourceId, Dictionary<string, string> properties = null, Dictionary<string, double> measurements = null)
        {
            if (quantity == 0)
            {
                return;
            }

            EntityType entityType = _gameData.Get<EntitySettings>(_gs.ch).Get(entityTypeId);

            IIdName entity = _entityService.Find(_gs.ch, entityTypeId, entityId);

            RewardSourceType rewardSource = _gameData.Get<RewardSourceSettings>(_gs.ch).Get(rewardSourceId);

            string entityTypeName = entityType?.GetAnalyticsName() ?? "unknown_entitytype_" + entityTypeId;
            string entityName = entity?.GetAnalyticsName() ?? "unknown_entity_" + entityId;

            string rewardSourceName = rewardSource?.GetAnalyticsName() ?? "unknown_reward_source_" + rewardSourceId;

            if (properties == null)
            {
                properties = new Dictionary<string, string>();
            }

            AddNonNullValue(properties, AnalyticsKeys.EntityTypeName, entityTypeName);
            AddNonNullValue(properties, AnalyticsKeys.EntityName, entityName);
            AddNonNullValue(properties, AnalyticsKeys.RewardSourceName, rewardSourceName);

            if (measurements == null)
            {
                measurements = new Dictionary<string, double>();
            }

            measurements[AnalyticsKeys.Quantity] = quantity;
            TrackEvent(eventName, properties, measurements);
        }

        /// <summary>
        /// Frame-level service tick. Hook this method into an engine update loop manager
        /// to ensure events are flushed periodically even if the game hasn't hit MaxBatchSize.
        /// </summary>
        public void UpdateService()
        {
            if (!_didInitialize || IsProcessingQueue || TelemetryQueue.IsEmpty)
            {
                return;
            }

            double elapsedTime = (DateTime.UtcNow - _lastSendTime).TotalSeconds;

            if (elapsedTime >= MaxBatchDelaySeconds || TelemetryQueue.Count >= MaxBatchSize)
            {
                ProcessQueue();
            }
        }
        protected void ProcessQueue()
        {
            if (TelemetryQueue.IsEmpty)
            {
                return;
            }

            IsProcessingQueue = true;
            _lastSendTime = DateTime.UtcNow;

            List<TelemetryEnvelope> batchList = new List<TelemetryEnvelope>();

            // Safely pull a localized batch up to your max payload threshold constraint
            while (batchList.Count < MaxBatchSize && TelemetryQueue.TryDequeue(out TelemetryEnvelope item))
            {
                batchList.Add(item);
            }

            if (batchList.Count == 0)
            {
                IsProcessingQueue = false;
                return;
            }

            // Configure the request options for the telemetry payload
            WebRequestOptions options = new WebRequestOptions
            {
                Method = HttpMethodType.Post,
                ContentType = HttpContentType.Json,
                JsonBody = batchList,
                MaxRetries = 1 // Prevent aggressive telemetry blocking retries inside the queue frame loop
            };

            // Include the specific encoding requirements in the custom headers dictionary if required by ingest
            options.Headers["Content-Type"] = "application/json; charset=utf-8";

            // Use SendSync with a completion callback to mirror the previous fire-and-forget logic
            _webRequestService.SendSync<string>(IngestionEndpoint, options, response =>
            {
                try
                {
                    if (!response.Success)
                    {
                        _logService.Warning($"[Telemetry Pipeline] Batch drop failure. Azure Endpoint responded with error: {response.ErrorMessage}");
                    }
                }
                finally
                {
                    IsProcessingQueue = false;

                    // Immediately re-evaluate pipeline loop if a substantial queue backlog is still waiting
                    if (TelemetryQueue.Count >= MaxBatchSize)
                    {
                        ProcessQueue();
                    }
                }
            });
        }

        public int SetupPriorityAscending()
        {
            return SetupPriorities.Logging;
        }

        public async Task PrioritySetup(CancellationToken token)
        {
            // ... Your existing ConnectionString parsing logic here ...

            _didInitialize = true;

            // Run recovery directly after successful initialization
            try
            {
                string cachePath = Path.Combine(Application.persistentDataPath, "UnsentTelemetry.json");
                if (File.Exists(cachePath))
                {
                    string cachedJson = File.ReadAllText(cachePath, Encoding.UTF8);
                    if (!string.IsNullOrWhiteSpace(cachedJson))
                    {
                        // Deserialize using your generic text tool setup
                        List<TelemetryEnvelope> recoveredEvents = _textSerializer.Deserialize<List<TelemetryEnvelope>>(cachedJson);

                        if (recoveredEvents != null)
                        {
                            foreach (TelemetryEnvelope oldEnvelope in recoveredEvents)
                            {
                                TelemetryQueue.Enqueue(oldEnvelope);
                            }

                            // Force an immediate processing loop run if we have recovered data
                            if (!IsProcessingQueue)
                            {
                                ProcessQueue();
                            }
                        }
                    }

                    // Delete the file immediately after recovery so we don't duplicate items if a crash occurs later
                    File.Delete(cachePath);
                }
            }
            catch (Exception ex)
            {
                _logService.Exception(ex, "[Telemetry] Error recovering offline telemetry cache.");
            }

            await Task.CompletedTask;
        }


        public void TrackAccumulatedRewards(AccumulatedRewards rewards, Dictionary<string, string> properties = null, Dictionary<string, double> measurements = null)
        {
            foreach (long rewardSourceId in rewards.Inflows.Keys)
            {
                foreach (Reward rew in rewards.Inflows[rewardSourceId])
                {
                    TrackEconomyEvent(AnalyticsEventNames.RewardInflow, rew.EntityTypeId, rew.EntityId, rew.Quantity, rewardSourceId, properties, measurements);
                }
            }
            foreach (long rewardSourceId in rewards.Outflows.Keys)
            {
                foreach (Reward rew in rewards.Outflows[rewardSourceId])
                {
                    TrackEconomyEvent(AnalyticsEventNames.RewardOutflow, rew.EntityTypeId, rew.EntityId, rew.Quantity, rewardSourceId, properties, measurements);
                }
            }
        }

        public async Task OnReset(CancellationToken token)
        {
            SaveUnsentEvents();
            await Task.CompletedTask;
        }

        public void OnQuit()
        {
            SaveUnsentEvents();
        }
        private void SaveUnsentEvents()
        {
            // If there is nothing to flush, clear out any old temp log data and exit
            if (TelemetryQueue.IsEmpty)
            {
                return;
            }

            try
            {
                List<TelemetryEnvelope> lingeringEvents = new List<TelemetryEnvelope>();
                while (TelemetryQueue.TryDequeue(out TelemetryEnvelope item))
                {
                    lingeringEvents.Add(item);
                }

                if (lingeringEvents.Count > 0)
                {
                    // Use a specific path for cached event data separate from standard runtime log files
                    string cachePath = Path.Combine(Application.persistentDataPath, "UnsentTelemetry.json");

                    // Serialize synchronously using your text tool wrapper
                    string jsonPayload = _textSerializer.SerializeToString(lingeringEvents);

                    // Blocking file write guarantees the data hits the disk before the thread dies
                    File.WriteAllText(cachePath, jsonPayload, Encoding.UTF8);
                }
            }
            catch (Exception ex)
            {
                _logService.Exception(ex, "[Telemetry] Failed to write fallback cache file on quit.");
            }

        }
    }
}