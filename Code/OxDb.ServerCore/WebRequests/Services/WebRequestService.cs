using OxDb.ServerCore.Setup;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Serialization.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.ServerCore.WebRequests.Services
{


    public class ResponseEnvelope<TResponse> where TResponse : class
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = null!;
        public string RawResponse { get; set; } = null!;
        private TResponse _response { get; set; } = null!;
        public TResponse Response => _response;

        public bool SetResponse(object obj)
        {
            if (obj is TResponse respo)
            {
                _response = respo;
                return true;
            }
            else
            {
                return false;
            }
        }
    }




    public interface IWebRequestService : IInitializable
    {
        Task<ResponseEnvelope<TResponse>> PostAsync<TRequest, TResponse>(string url, TRequest request) where TRequest : class where TResponse : class;
        Task<ResponseEnvelope<TResponse>> SendFormAsync<TResponse>(string url, Dictionary<string, string> formData) where TResponse : class;       
        Task<ResponseEnvelope<TResponse>> GetAsync<TResponse>(string url) where TResponse : class;
    }

    public class WebRequestService : IWebRequestService
    {
        private ITextSerializer _textSerializer = null;
        private IHttpClientFactory _clientFactory = null;
        private ILogService _logService = null;

        public async Task Initialize(CancellationToken token)
        {
            _clientFactory = DotNetServiceConfiguration.GetHttpClientFactory();
            await Task.CompletedTask;
        }

        protected async Task<ResponseEnvelope<TResponse>> PostContentEnvelopeAsync<TResponse>(string url, Func<HttpContent> contentFactory) where TResponse : class
        {

            ResponseEnvelope<TResponse> envelope = new ResponseEnvelope<TResponse>();
            const int maxRetries = 3;
            // HARDENING: Do NOT place the HttpClient itself inside a using block when derived from IHttpClientFactory
            HttpClient client = _clientFactory.CreateClient();

            for (int i = 0; i < maxRetries; i++)
            {
                // HARDENING: Ensure content allocation is safely scoped for disposal per-try
                using (HttpContent content = contentFactory())
                {
                    HttpResponseMessage response = null;
                    try
                    {
                        response = await client.PostAsync(url, content);

                        if (!response.IsSuccessStatusCode)
                        {
                            envelope.ErrorMessage = "HttpFailure: " + response.StatusCode + " -- " + await response.Content.ReadAsStringAsync();
                            _logService.Error("WebRequestRaw: " + await response.Content.ReadAsStringAsync());
                        }

                        // If it's a success, or a non-transient client side validation failure (4xx except timeouts)
                        if (response.IsSuccessStatusCode || ((int)response.StatusCode < 500 &&
                            response.StatusCode != System.Net.HttpStatusCode.RequestTimeout))
                        {
                            response.EnsureSuccessStatusCode();
                            string responseString = await response.Content.ReadAsStringAsync();

                            response.Dispose();
                            if (typeof(TResponse) != typeof(string))
                            {
                                try
                                {
                                    if (envelope.SetResponse(_textSerializer.Deserialize<TResponse>(responseString)))
                                    {

                                        envelope.Success = true;
                                        envelope.ErrorMessage = null!;
                                        return envelope;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    envelope.ErrorMessage = ex.Message + ": " + url + " Failed to deserialize type " + typeof(TResponse).Name;
                                    return envelope;
                                }
                            }
                            else
                            {
                                if (envelope.SetResponse(responseString))
                                {
                                    envelope.Success = true;
                                    envelope.ErrorMessage = null!;
                                    return envelope;
                                }
                            }
                        }
                    }
                    catch (HttpRequestException ex)
                    {
                        envelope.ErrorMessage = ex.GetType().ToString();
                        _logService.Exception(ex, $"WebRequestService.PostContentAsync Retry: {i}");
                    }
                    finally
                    {
                        // HARDENING: Always dispose the response message stream on failures/retries
                        response?.Dispose();
                    }
                }

                // HARDENING: Stop the thread sleep delay if we are on our absolute last attempt loop
                if (i < maxRetries - 1)
                {
                    await Task.Delay(500 * (int)Math.Pow(2, i));
                }
            }

            envelope.ErrorMessage = $"Failed to complete web request to  {url}";
            return envelope;
        }

        public async Task<ResponseEnvelope<TResponse>> PostAsync<TRequest, TResponse>(string url, TRequest request)
            where TRequest : class
            where TResponse : class
        {
            string serializedRequest = _textSerializer.SerializeToString(request);
            return await PostContentEnvelopeAsync<TResponse>(url, () => new StringContent(serializedRequest, System.Text.Encoding.UTF8, "application/json"));
        }

        public async Task<ResponseEnvelope<TResponse>> SendFormAsync<TResponse>(string url, Dictionary<string, string> formData) where TResponse : class
        {
            return await PostContentEnvelopeAsync<TResponse>(url, () => new FormUrlEncodedContent(formData));
        }

        protected async Task<string> PostContentAsync(string url, Func<HttpContent> contentFactory)
        {
            const int maxRetries = 3;
            // HARDENING: Do NOT place the HttpClient itself inside a using block when derived from IHttpClientFactory
            HttpClient client = _clientFactory.CreateClient();

            for (int i = 0; i < maxRetries; i++)
            {
                // HARDENING: Ensure content allocation is safely scoped for disposal per-try
                using (HttpContent content = contentFactory())
                {
                    HttpResponseMessage response = null;
                    try
                    {
                        response = await client.PostAsync(url, content);

                        if (!response.IsSuccessStatusCode)
                        {
                            _logService.Error("WebRequestRaw: " + await response.Content.ReadAsStringAsync());
                        }

                        // If it's a success, or a non-transient client side validation failure (4xx except timeouts)
                        if (response.IsSuccessStatusCode || ((int)response.StatusCode < 500 &&
                            response.StatusCode != System.Net.HttpStatusCode.RequestTimeout))
                        {
                            response.EnsureSuccessStatusCode();
                            string responseString = await response.Content.ReadAsStringAsync();

                            // Clean up response before returning memory string
                            response.Dispose();
                            return responseString;
                        }
                    }
                    catch (HttpRequestException ex)
                    {
                        _logService.Exception(ex, $"WebRequestService.PostContentAsync Retry: {i}");
                    }
                    finally
                    {
                        // HARDENING: Always dispose the response message stream on failures/retries
                        response?.Dispose();
                    }
                }

                // HARDENING: Stop the thread sleep delay if we are on our absolute last attempt loop
                if (i < maxRetries - 1)
                {
                    await Task.Delay(500 * (int)Math.Pow(2, i));
                }
            }

            throw new Exception($"Failed to complete web request to target endpoint: {url}");
        }

        public async Task<ResponseEnvelope<TResponse>> GetAsync<TResponse>(string url) where TResponse : class
        {

            ResponseEnvelope<TResponse> envelope = new Services.ResponseEnvelope<TResponse>();

            HttpClient client = _clientFactory.CreateClient();
            try
            {
                // For a basic GET, you pass just the URL string directly
                HttpResponseMessage response = await client.GetAsync(url);

                // Throws an exception if the server returns a 4xx or 5xx error
                response.EnsureSuccessStatusCode();

                string jsonString = await response.Content.ReadAsStringAsync();

                if (envelope.SetResponse(_textSerializer.Deserialize<TResponse>(jsonString)))
                {
                    envelope.Success = true;
                    return envelope;
                }

                envelope.ErrorMessage = "Failed to deserialize response to type " + typeof(TResponse).Name + " from GET to " + url;
                return envelope;
            }
            catch (HttpRequestException ex)
            {
                envelope.ErrorMessage = ex.GetType().Name + " -- " + "Network request failed for GET from " + url;
                _logService.Exception(ex, "Network request failed for GET from " + url);
                return envelope;
            }
            catch (Exception ex)
            {
                envelope.ErrorMessage = ex.GetType().Name + " -- " + " General exception for GET from " + url;
                return envelope;
                _logService.Exception(ex, "General exception for GET from " + url);
            }
        }
    }
}