using OxDb.ServerCore.Setup;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Serialization.Interfaces;

namespace OxDb.ServerCore.WebRequests.Services
{
    public interface IWebRequestService : IInitializable
    {
        Task<TResponse> PostAsync<TRequest, TResponse>(string url, TRequest request) where TRequest : class where TResponse : class;
        Task<TResponse> SendFormAsync<TResponse>(string url, Dictionary<string, string> formData) where TResponse : class;
        Task<string> PostStringAsync(string url, string requestString);
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

        public async Task<TResponse> PostAsync<TRequest, TResponse>(string url, TRequest request)
            where TRequest : class
            where TResponse : class
        {
            return _textSerializer.Deserialize<TResponse>(await PostStringAsync(url, _textSerializer.SerializeToString(request)));
        }

        public async Task<string> PostStringAsync(string url, string requestString)
        {
            return await PostContentAsync(url, () => new StringContent(requestString));
        }

        public async Task<TResponse> SendFormAsync<TResponse>(string url, Dictionary<string, string> formData) where TResponse : class
        {
            return _textSerializer.Deserialize<TResponse>(await PostContentAsync(url, () => new FormUrlEncodedContent(formData)));
        }

        protected async Task<string> PostContentAsync(string url, Func<HttpContent> contentFactory)
        {
            int maxRetries = 3;
            using (HttpClient client = _clientFactory.CreateClient())
            {

                for (int i = 0; i < maxRetries; i++)
                {
                    try
                    {
                        HttpResponseMessage response = await client.PostAsync(url, contentFactory());

                        if (response.IsSuccessStatusCode || (int)response.StatusCode < 500 &&
                            response.StatusCode != System.Net.HttpStatusCode.RequestTimeout)
                        {
                            response.EnsureSuccessStatusCode();
                            return await response.Content.ReadAsStringAsync();
                        }
                    }
                    catch (HttpRequestException ex)
                    {
                        _logService.Exception(ex, $"WebRequestService.PostContentAsync Retry: {i}");
                    }

                    if (i < maxRetries)
                    {
                        await Task.Delay(500 * (int)Math.Pow(2, i));
                    }
                }
            }

            throw new Exception($"Failed to complete web request to {url}");
        }
    }
}

