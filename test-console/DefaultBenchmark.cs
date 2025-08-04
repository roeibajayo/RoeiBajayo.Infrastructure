using BenchmarkDotNet.Attributes;
using Infrastructure.Utils.Threads;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestConsole;

[MemoryDiagnoser(false)]
public class DefaultBenchmark
{
    //[Params(1, 1000)]
    //public int Size;

    //public IEnumerable<int> Range => Enumerable.Range(0, 1000);

    [GlobalSetup]
    public void OnLoad()
    {

    }

    //[Benchmark]
    //public async Task<int> TokenSource()
    //{
    //    var tcs = new TaskCompletionSource<int>();
    //    Task.Run(async () =>
    //    {
    //        await Task.Delay(10);
    //        tcs.SetResult(13);
    //    });
    //    return await tcs.Task;
    //}

}
