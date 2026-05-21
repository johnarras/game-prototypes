using Assets.Scripts.Config;
using Assets.Scripts.Core.Interfaces;
using OxDb.SharedCore.Config.Constants;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Serialization.Interfaces;
using OxDb.SharedCore.Setup.Constants;
using OxDb.SharedCore.Utils;
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
    [Serializable]
    public struct TraceEnvelope
    {
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("time")] public string Time { get; set; }
        [JsonPropertyName("iKey")] public string IKey { get; set; }
        [JsonPropertyName("data")] public TraceDataWrapper Data { get; set; }
    }

    [Serializable]
    public struct TraceDataWrapper
    {
        [JsonPropertyName("baseType")] public string BaseType { get; set; }
        [JsonPropertyName("baseData")] public TraceData BaseData { get; set; }
    }

    [Serializable]
    public struct TraceData
    {
        [JsonPropertyName("ver")] public int Ver { get; set; }
        [JsonPropertyName("message")] public string Message { get; set; }
        [JsonPropertyName("severityLevel")] public int SeverityLevel { get; set; }
        [JsonPropertyName("properties")] public Dictionary<string, string> Properties { get; set; }
    }
}

namespace Assets.Scripts.Logalytics.Services
{
    public class ClientLogService : IClientQuitCleanup, ILogService
    {
        protected IClientConfigContainer _configContainer = null;
        private ITextSerializer _textSerializer = null;

        protected string IngestionEndpoint { get; set; }
        protected string InstrumentationKey { get; set; }
        protected ConcurrentQueue<TraceEnvelope> LoggingQueue { get; set; } = new ConcurrentQueue<TraceEnvelope>();
        protected bool IsProcessingQueue { get; set; }

        protected bool _didInitialize = false;

        public async Task Initialize(CancellationToken token)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// System-level entry point that translates Unity Engine log styles into cloud diagnostics.
        /// </summary>
        public void TrackTrace(string message, LogType logType, Dictionary<string, string> properties = null)
        {
            if (!_didInitialize)
            {
                return;
            }

            // Map standard Unity log engine states to Azure Severity integer scales
            int severityLevel = logType switch
            {
                LogType.Log => 1,         // Information
                LogType.Warning => 2,     // Warning
                LogType.Error => 3,       // Error
                LogType.Assert => 3,      // Error
                LogType.Exception => 4,   // Critical
                _ => 1
            };

            EnqueueTrace(message, severityLevel, properties);
        }

        /// <summary>
        /// Explicit diagnostic entry point for manual back-end message severity configuration.
        /// </summary>
        public void TrackTrace(string message, int severityLevel, Dictionary<string, string> properties = null)
        {
            if (!_didInitialize)
            {
                return;
            }

            EnqueueTrace(message, severityLevel, properties);
        }

        protected void EnqueueTrace(string message, int severityLevel, Dictionary<string, string> properties)
        {
            TraceEnvelope envelope = new TraceEnvelope
            {
                // App Insights routing contract specifies '.Message' for traces
                Name = $"Microsoft.ApplicationInsights.{InstrumentationKey}.Message",
                Time = DateTime.UtcNow.ToString("o"),
                IKey = InstrumentationKey,
                Data = new TraceDataWrapper
                {
                    BaseType = "MessageData",
                    BaseData = new TraceData
                    {
                        Ver = 2,
                        Message = message,
                        SeverityLevel = severityLevel,
                        Properties = properties ?? new Dictionary<string, string>()
                    }
                }
            };

            LoggingQueue.Enqueue(envelope);

            if (!IsProcessingQueue)
            {
                ProcessQueue();
            }
        }

        protected void ProcessQueue()
        {
            IsProcessingQueue = true;

            if (!LoggingQueue.TryDequeue(out TraceEnvelope nextEnvelope))
            {
                IsProcessingQueue = false;
                return;
            }

            // High performance string serialization via standard System.Text.Json (IL2CPP safe v8.0.x build)
            string jsonPayload = _textSerializer.SerializeToString(nextEnvelope);
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
                        // We use local engine routing warnings for communication dropouts to prevent looping exceptions
                        UnityEngine.Debug.LogWarning($"[Logging System] Ingestion failure: {request.error}");
                    }
                }
                finally
                {
                    request.Dispose();
                    ProcessQueue();
                }
            };
        }

        public int SetupPriorityAscending()
        {
            return SetupPriorities.Logging;
        }

        public async Task PrioritySetup(CancellationToken token)
        {
            string connectionString = _configContainer.Config.AppInsightsConnectionString;
#if UNITY_EDITOR
            Dictionary<string, string> kvDict = XmlUtils.ExtractAppConfigData(ConfigConstants.MainAppConfigPath);
            connectionString = kvDict[AppConfigKeys.AppInsightsConnectionString];
#endif
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
                    IngestionEndpoint = $"{endpoint}/v2.1/track";
                }
            }

            if (string.IsNullOrEmpty(InstrumentationKey) || string.IsNullOrEmpty(IngestionEndpoint))
            {
                UnityEngine.Debug.LogError("Failed to parse infrastructure parameters for logging runtime configuration.");
                return;
            }

            _didInitialize = true;
            await Task.CompletedTask;
        }

        public void OnQuit()
        {
            string fullPath = Path.Combine(Application.persistentDataPath, "LogFile.txt");
            StringBuilder sb = new StringBuilder();
            File.WriteAllText(fullPath, sb.ToString());
        }

        public void Info(string txt)
        {
        }

        public void Warning(string txt)
        {
        }

        public void Debug(string txt)
        {
        }

        public void Error(string txt)
        {
        }

        public void Exception(Exception e, string txt)
        {
        }
    }
}