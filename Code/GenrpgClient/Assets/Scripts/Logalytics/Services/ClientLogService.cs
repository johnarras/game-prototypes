using Assets.Scripts.Core.Interfaces;
using Assets.Scripts.Logalytics.Utils;
using OxDb.SharedCore.Core.Constants;
using OxDb.SharedCore.Environments.Constants;
using OxDb.SharedCore.Logalytics.Constants;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Serialization.Services;
using OxDb.SharedCore.Setup.Constants;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts.Logalytics.Services
{
    [Serializable]
    public struct TraceEnvelope
    {
        public string name { get; set; }
        public string time { get; set; }
        public int ver { get; set; }
        public string iKey { get; set; }
        public TraceDataWrapper data { get; set; }
    }

    [Serializable]
    public struct TraceDataWrapper
    {
        public string baseType { get; set; }
        public TraceData baseData { get; set; }
    }

    [Serializable]
    public struct TraceData
    {
        public int ver { get; set; }
        public string message { get; set; }
        public int severityLevel { get; set; }
        public Dictionary<string, string> properties { get; set; }
    }

    public class LogSeverityLevels
    {
        public const int Verbose = 0;
        public const int Information = 1;
        public const int Warning = 2;
        public const int Error = 3;
        public const int Critical = 4;

    }

    public class ClientLogService : IClientQuitCleanup, ILogService
    {
        private IClientLogalyticsService _logalyticsService = null;
        private IClientWebService _webService = null;

        protected string IngestionEndpoint { get; set; }
        protected string InstrumentationKey { get; set; }
        protected ConcurrentQueue<TraceEnvelope> LoggingQueue { get; set; } = new ConcurrentQueue<TraceEnvelope>();
        protected bool IsProcessingQueue { get; set; }

        protected bool _didInitialize = false;


        private bool _verboseLogging = false;

        private ClientConfig _config = null;

        NewtonsoftTextSerializer serializer = new NewtonsoftTextSerializer();


        // Do NOT use any external DI stuff in here because this is basically the first thing set up when the client runs.
        public ClientLogService(ClientConfig config)
        {

            _config = config;
            _verboseLogging = config.Flags.HasFlag(ClientPlayerFlags.VerboseLogging);
            if (GameModeUtils.IsPureClientMode(config.GameMode))
            {
                _didInitialize = false;
                return;
            }

            string connectionString = LogalyticsUtils.GetLogConnectionString(config);

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
                UnityEngine.Debug.LogError("Failed to parse infrastructure parameters for logging runtime configuration.");
                return;
            }

            _didInitialize = true;
        }

        public async Task Initialize(CancellationToken token)
        {
            if (GameModeUtils.IsPureClientMode(_config.GameMode))
            {
                return;
            }

            await Task.CompletedTask;
        }

        protected void EnqueueTrace(string message, int severityLevel)
        {
            if (!_didInitialize)
            {
                return;
            }

            int minSeverityLevel = LogSeverityLevels.Information;
            if (EnvNames.IsProdEnv(_config.Env))
            {
                minSeverityLevel = LogSeverityLevels.Warning;
            }

            if (severityLevel < minSeverityLevel)
            {
                return;
            }

            // Clean input text against common log text anomaly loops (\r\r\n and U+00A0 non-breaking spaces)
            string sanitizedMessage = string.IsNullOrEmpty(message)
                ? string.Empty
                : message.Replace("\r\r\n", "\n").Replace("\u00A0", " ");

            Dictionary<string, string> properties = new Dictionary<string, string>();

            if (_logalyticsService != null)
            {
                _logalyticsService.GetDefaultLogalyticsDimensions();
            }
            if (properties == null)
            {
                properties = new Dictionary<string, string>();
            }

            if (_webService != null)
            {
                properties[LogalyticsKeys.RequestId] = _webService.GetUserRequestId();
            }
            // Strip null tracking properties before serialization to prevent engine 400 validation drops
            if (properties.Any(x => x.Value == null))
            {
                List<string> keys = properties.Keys.ToList();
                foreach (string key in keys)
                {
                    if (properties[key] == null)
                    {
                        properties.Remove(key);
                    }
                }
            }

            // Safely embed severity as a structured string property to isolate raw json structural issues

            TraceEnvelope envelope = new TraceEnvelope
            {
                name = "Message",
                time = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.ffffffZ"),
                ver = 1,
                iKey = InstrumentationKey,
                data = new TraceDataWrapper
                {
                    baseType = "MessageData",
                    baseData = new TraceData
                    {
                        ver = 2,
                        message = sanitizedMessage,
                        severityLevel = severityLevel,
                        properties = properties
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
            if (!_didInitialize)
            {
                return;
            }

            IsProcessingQueue = true;

            if (!LoggingQueue.TryDequeue(out TraceEnvelope nextEnvelope))
            {
                IsProcessingQueue = false;
                return;
            }

            List<TraceEnvelope> envList = new List<TraceEnvelope>() { nextEnvelope };

            string jsonPayload = serializer.SerializeToString(envList);
            byte[] rawBytes = Encoding.UTF8.GetBytes(jsonPayload);

            UnityWebRequest request = new UnityWebRequest(IngestionEndpoint, "POST");
            request.uploadHandler = new UploadHandlerRaw(rawBytes);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json; charset=utf-8");

            UnityWebRequestAsyncOperation operation = request.SendWebRequest();

            operation.completed += _ =>
            {
                try
                {
                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        UnityEngine.Debug.LogWarning($"[Logging System] Ingestion failure: {request.error} | Response Code: {request.responseCode}");
                    }
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogWarning($"[Logging System] Post-processing exception: {ex.Message}");
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
            EnqueueTrace(txt, LogSeverityLevels.Information);
            UnityEngine.Debug.Log(txt);
        }

        public void Verbose(string txt)
        {
            if (_verboseLogging)
            {
                EnqueueTrace(txt, LogSeverityLevels.Information);
                UnityEngine.Debug.Log(txt);
            }
        }

        public void Warning(string txt)
        {
            EnqueueTrace(txt, LogSeverityLevels.Warning);
            UnityEngine.Debug.LogWarning(txt);
        }

        public void Debug(string txt)
        {
            EnqueueTrace(txt, LogSeverityLevels.Verbose);
            UnityEngine.Debug.Log(txt);
        }

        public void Error(string txt)
        {
            EnqueueTrace(txt, LogSeverityLevels.Error);
            UnityEngine.Debug.LogError(txt);
        }

        public void Exception(Exception e, string txt)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(txt + '\n');
            sb.Append(e.Message + '\n');

            string[] stackTrace = e.StackTrace.Split('\n');
            for (int i = 0; i < 3 && i < stackTrace.Length; i++)
            {
                sb.Append(stackTrace[i] + '\n');
            }

            EnqueueTrace(sb.ToString(), LogSeverityLevels.Critical);
            UnityEngine.Debug.LogException(e);
        }
    }
}