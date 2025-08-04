using Infrastructure.Utils.Threads;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Utils.WebSocket;

public abstract class WebSocketHostedService(ILogger logger) 
    : IHostedService, IDisposable
{
    private WebSocketClient? client;
    private bool initialized = false;
    private int waitingForReady = 0;
    private SemaphoreSlim? readyLocker = null;

    protected bool Ready { get; private set; }

    protected virtual bool AutoReady =>
        true;
    protected virtual bool AutoStart =>
        false;

    public WebSocketState State =>
        client?.State ?? WebSocketState.None;

    public virtual Task StartAsync(CancellationToken cancellationToken)
    {
        if (AutoStart)
            _ = TryStartAsync(cancellationToken);

        return Task.CompletedTask;
    }
    public virtual Task StopAsync(CancellationToken cancellationToken)
    {
        Close();
        return Task.CompletedTask;
    }

    protected async Task TryStartAsync(CancellationToken cancellationToken)
    {
        client ??= new WebSocketClient();

        if (client.State == WebSocketState.Open)
            return;

        _ = Tasks.StartNow(async () =>
        {
            if (initialized)
                return;

            initialized = true;

            try
            {
                await ConnectAsync(client, cancellationToken);

                if (AutoReady)
                    SetReady();

                if (client is not null)
                {
                    await foreach (var message in client.ReceiveAllAsync(cancellationToken))
                    {
                        if (message is null)
                            continue;

                        ProcessMessage(message);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Connection error");
            }

            Close();
        }, cancellationToken);

        await WaitForReadyAsync(cancellationToken);
    }
    protected void SetReady()
    {
        Ready = true;

        while (waitingForReady-- > 0)
            readyLocker?.Release();
    }

    protected async Task SendJsonMessageAsync(object json, bool ignoreReady = false, CancellationToken? cancellationToken = null) =>
        await SendMessageAsync(JsonSerializer.Serialize(json), ignoreReady, cancellationToken);
    protected async Task SendMessageAsync(string message, bool ignoreReady = false, CancellationToken? cancellationToken = null)
    {
        cancellationToken ??= CancellationToken.None;

        await TryStartAsync(cancellationToken.Value);

        if (!ignoreReady)
        {
            await WaitForReadyAsync(cancellationToken.Value);
        }

        if (client is not null)
            await client.SendAsync(message, cancellationToken);
    }

    private async Task WaitForReadyAsync(CancellationToken cancellationToken)
    {
        if (Ready)
            return;

        waitingForReady++;
        readyLocker ??= new SemaphoreSlim(0, 1);
        await readyLocker.WaitAsync(cancellationToken);
    }

    protected abstract Task ConnectAsync(WebSocketClient client, CancellationToken cancellationToken);
    protected abstract void ProcessMessage(string message);

    public virtual async Task ResetAsync(CancellationToken cancellationToken = default)
    {
        Close();
        await TryStartAsync(cancellationToken);
    }

    public void Close()
    {
        if (client is not null)
        {
            client.Dispose();
            client = null;
        }

        initialized = false;
        Ready = false;

        waitingForReady = 0;

        if (readyLocker is not null)
        {
            readyLocker.Dispose();
            readyLocker = null;
        }

        logger.LogTrace("Connection closed safely");
    }
    public void Dispose()
    {
        Close();
        GC.SuppressFinalize(this);
    }
}
