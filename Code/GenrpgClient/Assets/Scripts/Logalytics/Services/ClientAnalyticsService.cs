using Assets.Scripts.Core.Interfaces;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Serialization.Interfaces;
using OxDb.SharedCore.Setup.Constants;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts.Logalytics.Services
{
    #region Telemetry JSON Schema Structures

    [Serializable]
    public struct TelemetryEnvelope
    {
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("time")] public string Time { get; set; }
        [JsonPropertyName("iKey")] public string IKey { get; set; }
        [JsonPropertyName("data")] public TelemetryData Data { get; set; }
    }

    [Serializable]
    public struct TelemetryData
    {
        [JsonPropertyName("baseType")] public string BaseType { get; set; }
        [JsonPropertyName("baseData")] public EventData BaseData { get; set; }
    }

    [Serializable]
    public struct EventData
    {
        [JsonPropertyName("ver")] public int Ver { get; set; }
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("properties")] public Dictionary<string, string> Properties { get; set; }
    }

    #endregion

    public class ClientAnalyticsService : IAnalyticsService, IClientQuitCleanup
    {
        protected IClientConfigContainer _configContainer = null;
        private ITextSerializer _textSerializer = null;
        private IClientUpdateService _updateService = null;

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
            _updateService.AddUpdate(this, UpdateService, UpdateTypes.Late, token);
            await Task.CompletedTask;
        }

        public void TrackEvent(string eventName, Dictionary<string, string> properties = null)
        {
            // Fixed inverse logic initialization check
            if (!_didInitialize)
            {
                return;
            }

            TelemetryEnvelope envelope = new TelemetryEnvelope
            {
                Name = $"Microsoft.ApplicationInsights.{InstrumentationKey}.Event",
                Time = DateTime.UtcNow.ToString("o"),
                IKey = InstrumentationKey,
                Data = new TelemetryData
                {
                    BaseType = "EventData",
                    BaseData = new EventData
                    {
                        Ver = 2,
                        Name = eventName,
                        Properties = properties ?? new Dictionary<string, string>()
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

            // Route through your native text serializer tool to output a clean telemetry JSON array structure ([...])
            string jsonPayload = _textSerializer.SerializeToString(batchList);
            byte[] rawBytes = Encoding.UTF8.GetBytes(jsonPayload);

            UnityWebRequest request = new UnityWebRequest(IngestionEndpoint, "POST");
            request.uploadHandler = new UploadHandlerRaw(rawBytes);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            UnityWebRequestAsyncOperation operation = request.SendWebRequest();

            operation.completed += _ =>
            {
                try
                {
                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        UnityEngine.Debug.LogWarning($"[Telemetry Pipeline] Batch drop failure. Azure Endpoint responded with error: {request.error}");
                    }
                }
                finally
                {
                    request.Dispose();
                    IsProcessingQueue = false;

                    // Immediately re-evaluate pipeline loop if a substantial queue backlog is still waiting
                    if (TelemetryQueue.Count >= MaxBatchSize)
                    {
                        ProcessQueue();
                    }
                }
            };
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
                UnityEngine.Debug.LogWarning($"[Telemetry] Error recovering offline telemetry cache: {ex.Message}");
            }

            await Task.CompletedTask;
        }

        public void OnQuit()
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
                UnityEngine.Debug.LogError($"[Telemetry] Failed to write fallback cache file on quit: {ex.Message}");
            }
        }

        public void TrackEvent(string eventType, string eventId, string eventSubtype = null, Dictionary<string, string> extraData = null)
        {
            // Map your secondary custom overload layout directly into standard properties payload dictionary
            Dictionary<string, string> properties = extraData ?? new Dictionary<string, string>();

            if (!properties.ContainsKey("Id")) properties.Add("Id", eventId);
            if (!string.IsNullOrEmpty(eventSubtype) && !properties.ContainsKey("Subtype")) properties.Add("Subtype", eventSubtype);

            TrackEvent(eventType, properties);
        }
    }
}