using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleAppPollyProber
{
    public interface IProber
    {
        Task<bool> ProbeAsync(Uri[] urls, CancellationToken cancel);
    }
}
