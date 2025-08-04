using BenchmarkDotNet.Running;
using Infrastructure.Utils;
using Infrastructure.Utils.TextToSpeech.Interfaces;
using Infrastructure.Utils.TextToSpeech.Models;
using Infrastructure.Utils.Threads;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TestConsole;

public class Program
{
    public static void Main(string[] args)
    {
        _ = BenchmarkRunner.Run<DefaultBenchmark>();
    }

    //public static void Main(string[] args)
    //{
    //    var builder = Host.CreateApplicationBuilder();
    //    builder.Services.AddInfrastructureServices();
    //    builder.Services.AddHostedService<Executer>();

    //    using IHost host = builder.Build();
    //    host.Run();
    //}

    private class Executer(ILogger<Executer> logger, ITextToSpeech textToSpeech) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            Console.WriteLine("Finished");
        }
    }
}
