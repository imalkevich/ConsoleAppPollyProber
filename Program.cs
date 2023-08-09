using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;

namespace ConsoleAppPollyProber
{
    internal class Program
    {
        private static IAsyncPolicy<HttpResponseMessage> CreateProbeRequestPolicy(IServiceProvider serviceProvider, HttpRequestMessage _)
        {
            var individualTimeoutPolicy = Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(5));

            return individualTimeoutPolicy;
        }

        static async Task Main(string[] args)
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(120));

            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;

                if (cts.IsCancellationRequested)
                    return;

                cts.Cancel();
            };

            var services = new ServiceCollection();
            services
                .AddLogging(builder =>
                    builder.AddSimpleConsole(options =>
                    {
                        options.IncludeScopes = true;
                        options.SingleLine = true;
                        options.TimestampFormat = "hh:mm:ss.fff ";
                    })
                    .SetMinimumLevel(LogLevel.Debug)
                );

            services
                .AddHttpClient<IProber, Prober>(Prober.HttpClientName)
                .AddPolicyHandler(CreateProbeRequestPolicy);

            services
                .AddSingleton<IProber, Prober>()
                .AddSingleton<IProberRunner, ProberRunner>();

            var serviceProvider = services.BuildServiceProvider();

            var probeRunner = serviceProvider.GetRequiredService<IProberRunner>();

            var urls = new[]
            {
                new Uri("https://localnortheurope509caee30d55b41773161e94.azureedge.net/62653174-c7cd-484e-b88d-ccb15a067407/media.m3u8"),
                new Uri("https://localnortheurope50c8c0c8ad365be7fc2bef34.azureedge.net/62653174-c7cd-484e-b88d-ccb15a067407/media.m3u8"),
            };

            await probeRunner.WaitAsync(urls, cts.Token);

            Console.WriteLine("Done,.press any key to close the window...");

            Console.ReadLine();
        }
    }
}
