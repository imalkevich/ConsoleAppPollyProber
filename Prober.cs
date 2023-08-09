using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ConsoleAppPollyProber
{
    public class Prober : IProber
    {
        public const string HttpClientName = nameof(Prober);

        private readonly HttpClient _httpClient;
        private readonly ILogger<Prober> _logger;

        public Prober(HttpClient httpClient,
            ILogger<Prober> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<bool> ProbeAsync(Uri[] urls, CancellationToken cancel)
        {
            var probeMediaStreamTasks = urls.Select(requestUri =>
            {
                return Task.Run(async delegate
                {
                    try
                    {
                        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

                        var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancel);

                        _logger.LogInformation("Response from {url} - {status}", requestUri, response.StatusCode);

                        return response.IsSuccessStatusCode;
                    }
                    catch (Exception ex)
                    {
                        // We catch all exceptions and report them - reason of failure does not matter for the operation result.
                        _logger.LogWarning(ex, "Failed to query {url} ({message})", requestUri, ex.Message);

                        return false;
                    }
                }, cancel);
            });

            var probeResults = await Task.WhenAll(probeMediaStreamTasks);

            var isReady = probeResults.Any(x => x);

            _logger.LogInformation("Is ready: {isReady} ({ready}/{total})", isReady, probeResults.Count(x => x), probeResults.Length);

            return isReady;
        }
    }
}
