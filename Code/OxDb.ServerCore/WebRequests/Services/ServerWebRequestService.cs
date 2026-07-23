using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Serialization.Interfaces;
using OxDb.SharedCore.WebRequests.Services;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
// ... (System using declarations placed above namespace block)

public class ServerWebRequestService : BaseWebRequestService
{
    private readonly IHttpClientFactory _clientFactory = null!;


    CancellationToken _token = default;
    public override async Task Initialize(CancellationToken token)
    {
        _token = token;
        await Task.CompletedTask;
    }

    protected override async ValueTask<RawHttpResponse> ExecuteTransportAsync(string url, WebRequestOptions options, CancellationToken token)
    {
        HttpClient client = _clientFactory.CreateClient();
        client.Timeout = TimeSpan.FromMilliseconds(options.TimeoutMilliseconds);

        HttpMethod method = options.Method switch
        {
            HttpMethodType.Post => HttpMethod.Post,
            HttpMethodType.Put => HttpMethod.Put,
            HttpMethodType.Delete => HttpMethod.Delete,
            _ => HttpMethod.Get
        };

        using (HttpRequestMessage request = new HttpRequestMessage(method, url))
        {
            if (!string.IsNullOrEmpty(options.AuthToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", options.AuthToken);
            }

            foreach (KeyValuePair<string, string> header in options.Headers)
            {
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            if (options.Method == HttpMethodType.Post || options.Method == HttpMethodType.Put)
            {
                if (options.ContentType == HttpContentType.Json && options.JsonBody != null)
                {
                    string json = _textSerializer.SerializeToString(options.JsonBody);
                    request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }
                else if (options.ContentType == HttpContentType.FormUrlEncoded && options.FormBody != null)
                {
                    request.Content = new FormUrlEncodedContent(options.FormBody);
                }
            }

            using (HttpResponseMessage response = await client.SendAsync(request, token))
            {
                byte[] bytes = await response.Content.ReadAsByteArrayAsync(token);
                return new RawHttpResponse
                {
                    IsSuccess = response.IsSuccessStatusCode,
                    StatusCode = (int)response.StatusCode,
                    Data = bytes,
                    ErrorMessage = response.IsSuccessStatusCode ? null : response.ReasonPhrase
                };
            }
        }
    }
}