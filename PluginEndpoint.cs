using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

public class PluginEndpoint
{
    private readonly ILogger _logger;
    private readonly AppConfig _appConfig;

    public PluginEndpoint(AppConfig appConfig, ILoggerFactory loggerFactory)
    {
        _appConfig = appConfig;
        _logger = loggerFactory.CreateLogger<PluginEndpoint>();
    }
    
    [Function("WellKnownAIPlugin")]
    public async Task<HttpResponseData> WellKnownAIPlugin(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route=".well-known/ai-plugin.json")] HttpRequestData req)
    {
        var toReturn = new AIPlugin();
        toReturn.Api.Url = $"{req.Url.Scheme}://{req.Url.Host}:{req.Url.Port}/swagger.json";

        var r = req.CreateResponse(HttpStatusCode.OK);
        await r.WriteAsJsonAsync(toReturn);
        return r;
    }    

    [OpenApiOperation(operationId: "Search", tags: new[] { "WebSearchFunction" }, Description = "Searches the web for the given query.")]
    [OpenApiParameter(name: "Query", Description = "The query", Required = true, In = ParameterLocation.Query)]
    [OpenApiParameter(name: "NumResults", Description = "The maximum number of results to return", Required = true, In = ParameterLocation.Query)]
    [OpenApiParameter(name: "Offset", Description = "The number of results to skip", Required = false, In = ParameterLocation.Query)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "Returns a collection of search results with the name, URL and snippet for each.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(string), Description = "Returns the error of the input.")]
    [Function("WebSearch")]
    public async Task<HttpResponseData> WebSearch([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route="search")] HttpRequestData req)
    {
        var query = req.Query("Query").FirstOrDefault();

        if (query == null)
        {
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        var numResults = req.Query("NumResults").FirstOrDefault();

        if (numResults == null)
        {
            return req.CreateResponse(HttpStatusCode.BadRequest);
        }

        var offset = req.Query("Offset").FirstOrDefault();
        var offsetInt = offset == null ? 0 : int.Parse(offset);

        _logger.LogInformation($"Starting search for: {query}");

        using (var httpClient = new HttpClient())
        {
            var uri = new Uri($"https://api.bing.microsoft.com/v7.0/search?q={Uri.EscapeDataString(query)}&count={numResults}&offset={offsetInt}");
            httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _appConfig.BingApiKey);

            var json = await httpClient.GetStringAsync(uri);

            _logger.LogInformation($"Search completed for: {query}");

            BingSearchResponse? data = JsonSerializer.Deserialize<BingSearchResponse>(json);

            WebPage[]? results = data?.WebPages?.Value;

            var r = req.CreateResponse(HttpStatusCode.OK);
            r.Headers.Add("Content-Type", "text/plain");
            await r.WriteStringAsync(results == null ? 
                "No results found" : 
                string.Join(",", results.Select(x => $"[NAME]{x.Name}[END NAME] [URL]{x.Url}[END URL] [SNIPPET]{x.Snippet}[END SNIPPET]")));
            return r;
        }
    }

    [SuppressMessage("Performance", "CA1812:Internal class that is apparently never instantiated",
        Justification = "Class is instantiated through deserialization.")]
    private sealed class BingSearchResponse
    {
        [JsonPropertyName("webPages")]
        public WebPages? WebPages { get; set; }
    }

    [SuppressMessage("Performance", "CA1812:Internal class that is apparently never instantiated",
        Justification = "Class is instantiated through deserialization.")]
    private sealed class WebPages
    {
        [JsonPropertyName("value")]
        public WebPage[]? Value { get; set; }
    }

    [SuppressMessage("Performance", "CA1812:Internal class that is apparently never instantiated",
        Justification = "Class is instantiated through deserialization.")]
    private sealed class WebPage
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        [JsonPropertyName("snippet")]
        public string Snippet { get; set; } = string.Empty;
    }
}
