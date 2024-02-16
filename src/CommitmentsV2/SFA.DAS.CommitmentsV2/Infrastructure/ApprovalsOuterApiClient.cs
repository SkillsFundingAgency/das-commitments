using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi;

namespace SFA.DAS.CommitmentsV2.Infrastructure;

public class ApprovalsOuterApiClient : IApprovalsOuterApiClient
{
    private readonly HttpClient _httpClient;
    private readonly AsyncRetryPolicy _asyncRetryPolicy;
    private readonly ApprovalsOuterApiConfiguration _config;
    private readonly ILogger<ApprovalsOuterApiClient> _logger;

    public ApprovalsOuterApiClient (HttpClient httpClient, ApprovalsOuterApiConfiguration config, ILogger<ApprovalsOuterApiClient> logger)
    {
        _config = config;
        _logger = logger;
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(_config.BaseUrl);
        _asyncRetryPolicy = GetRetryPolicy();
    }

    public async Task<TResponse> Get<TResponse>(IGetApiRequest request) 
    {
        _logger.LogInformation("Calling Outer API base {BaseUrl}, url {GetUrl}", _config.BaseUrl, request.GetUrl);

        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, request.GetUrl);

        AddHeaders(httpRequestMessage);

        var response = await _httpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);

        if (response.StatusCode.Equals(HttpStatusCode.NotFound))
        {
            _logger.LogInformation("URL {Url} found nothing", request.GetUrl);
            return default;
        }

        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation("URL {Url} returned a response", request.GetUrl);
            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<TResponse>(json);    
        }

        _logger.LogInformation("URL {Url} returned a response {StatusCode}", request.GetUrl, response.StatusCode);
        response.EnsureSuccessStatusCode();
            
        return default;
    }

    public async Task<TResponse> GetWithRetry<TResponse>(IGetApiRequest request)
    {
        return await _asyncRetryPolicy.ExecuteAsync(async() => await Get<TResponse>(request));
    }

    public async Task<ApiResponse<TResponse>> PostWithResponseCode<TData, TResponse>(IPostApiRequest<TData> request, bool includeResponse = true) where TData : class, new() 
    {
        _logger.LogInformation("Posting to Outer API base {BaseUrl}, url {PostUrl}", _config.BaseUrl, request.PostUrl);

        var stringContent = request.Data != null ? new StringContent(JsonConvert.SerializeObject(request.Data), System.Text.Encoding.UTF8, "application/json") : null;

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, request.PostUrl);
        requestMessage.Content = stringContent;

        AddHeaders(requestMessage);

        var response = await _httpClient.SendAsync(requestMessage).ConfigureAwait(false);

        var errorContent = "";
        var responseBody = (TResponse)default;

        var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogInformation("URL {URL} return error status {StatusCode}", request.PostUrl, response.StatusCode);
            errorContent = json;
        }
        else if (includeResponse)
        {
            _logger.LogInformation("URL {PostUrl} returned a response", request.PostUrl);
            responseBody = JsonConvert.DeserializeObject<TResponse>(json);
        }

        var postWithResponseCode = new ApiResponse<TResponse>(responseBody, response.StatusCode, errorContent);

        return postWithResponseCode;
    }

    private void AddHeaders(HttpRequestMessage httpRequestMessage)
    {
        httpRequestMessage.Headers.Add("Ocp-Apim-Subscription-Key", _config.Key);
        httpRequestMessage.Headers.Add("X-Version", "1");
    }

    private static AsyncRetryPolicy GetRetryPolicy()
    {
        const int maxRetryAttempts = 3;
        var pauseBetweenFailures = TimeSpan.FromSeconds(2);

        return Policy
            .Handle<HttpRequestException>()
            .WaitAndRetryAsync(maxRetryAttempts, i => pauseBetweenFailures);
    }
}

public interface IPostApiRequest<TData>
{
    [JsonIgnore]
    string PostUrl { get; }
    TData Data { get; set; }
}

public class ApiResponse<TResponse>
{
    public TResponse Body { get; }
    public HttpStatusCode StatusCode { get; }
    public string ErrorContent { get; }

    public ApiResponse(TResponse body, HttpStatusCode statusCode, string errorContent)
    {
        Body = body;
        StatusCode = statusCode;
        ErrorContent = errorContent;
    }
}