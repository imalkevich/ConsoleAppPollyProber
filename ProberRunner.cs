using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ConsoleAppPollyProber
{
    public class ProberRunner : IProberRunner
    {
        private readonly IProber _prober;
        private readonly ILogger<ProberRunner> _logger;

        public ProberRunner(IProber prober, ILogger<ProberRunner> logger)
        {
            _prober = prober;
            _logger = logger;
        }

        public async Task WaitAsync(Uri[] urls, CancellationToken cancel)
        {
            _logger.LogInformation("Start prober with the following URLS: {urls}", string.Join(", ", urls.Select(x => x.AbsoluteUri)));

            var counter = 0;

            while (!cancel.IsCancellationRequested)
            {
                counter++;

                _logger.LogInformation("Probing - attempt #{counter}", counter);

                var isReady = await _prober.ProbeAsync(urls, cancel);

                if (isReady)
                {
                    break;
                }
            }

            _logger.LogInformation("Success after {counter} attempts", counter);
        }
    }
}
