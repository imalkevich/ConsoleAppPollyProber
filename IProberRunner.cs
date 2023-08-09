using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleAppPollyProber
{
    public interface IProberRunner
    {
        Task WaitAsync(Uri[] urls, CancellationToken cancel);
    }
}
