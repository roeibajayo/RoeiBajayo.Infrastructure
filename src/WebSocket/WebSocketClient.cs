using RoeiBajayo.Infrastructure.Threads;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace RoeiBajayo.Infrastructure.WebSocket;

/// <summary>
/// Thread-safe WebSocket client
/// </summary>
public class WebSocketClient : IDisposable
{
    private SemaphoreSlim? startLocker = null;
    private bool started = false;
    private ClientWebSocket? client = null;
    private Channel<byte[]>? channel = null;

    public WebSocketState State => client?.State ?? WebSocketState.None;

    public async Task StartAsync(string url,
        IDictionary<string, object>? headers = null,
        CancellationToken? cancellationToken = null)
    {
        if (started)
            startLocker ??= new(0, 1);

        cancellationToken ??= CancellationToken.None;

        if (startLocker is not null)
            await startLocker.WaitAsync(cancellationToken.Value);

        if (!started)
        {
            started = true;
            client = new();
            channel = Channel.CreateUnbounded<byte[]>(new UnboundedChannelOptions
            {
                SingleWriter = true,
                SingleReader = true
            });
        }

        try
        {
            if (headers is not null)
            {
                foreach (var (key, value) in headers)
                {
                    client!.Options.SetRequestHeader(key, value.ToString());
                }
            }

            await client!.ConnectAsync(new Uri(url), cancellationToken.Value);

            if (client.State != WebSocketState.Open)
            {
                started = false;
                client?.Dispose();
                throw new InvalidOperationException("WebSocket is not open");
            }

            _ = Tasks.StartNow(async () =>
            {
                while (client.State is WebSocketState.Open or WebSocketState.CloseSent)
                {
                    var buffer = new ArraySegment<byte>(ArrayPool<byte>.Shared.Rent(16 * 1024));
                    using var message = new MemoryStream();
                    while (client.State is WebSocketState.Open or WebSocketState.CloseSent)
                    {
                        var result = await client.ReceiveAsync(buffer, CancellationToken.None);

                        if (result.MessageType == WebSocketMessageType.Close)
                            throw new InvalidOperationException("WebSocket is not open");

                        message.Write(buffer.Array!, 0, result.Count);

                        if (result.EndOfMessage)
                        {
                            await channel!.Writer.WriteAsync(message.ToArray());
                            break;
                        }
                    }
                }
            });
        }
        finally
        {
            startLocker?.Release();
        }
    }

    public IAsyncEnumerable<byte[]> ReceiveAllBytesAsync(CancellationToken token)
    {
        if (!started || client!.State != WebSocketState.Open)
            throw new InvalidOperationException("WebSocket is not open");

        return channel!.Reader.ReadAllAsync(token);
    }
    public async IAsyncEnumerable<string?> ReceiveAllAsync([EnumeratorCancellation] CancellationToken token)
    {
        await foreach (var message in ReceiveAllBytesAsync(token))
        {
            yield return BytesToString(message);
        }
    }

    public async Task<byte[]> ReceiveBytesAsync(CancellationToken token)
    {
        if (!started || client!.State != WebSocketState.Open)
            throw new InvalidOperationException("WebSocket is not open");

        return await channel!.Reader.ReadAsync(token);
    }
    public async Task<string?> ReceiveAsync(CancellationToken token)
    {
        var bytes = await ReceiveBytesAsync(token);
        return BytesToString(bytes);
    }

    private static string? BytesToString(byte[]? bytes)
    {
        if (bytes is null or { Length: 0 })
            return null;

        return Encoding.UTF8.GetString(bytes);
    }

    public async Task SendJsonAsync(object message, CancellationToken? token = null)
    {
        if (!started || client!.State != WebSocketState.Open)
            throw new InvalidOperationException("WebSocket is not open");

        var json = JsonSerializer.Serialize(message);
        await SendAsync(json, token);
    }
    public async Task SendAsync(string message, CancellationToken? token = null)
    {
        if (!started || client!.State != WebSocketState.Open)
            throw new InvalidOperationException("WebSocket is not open");

        var bytes = Encoding.UTF8.GetBytes(message);
        var buffer = new ArraySegment<byte>(bytes);
        await client.SendAsync(buffer, WebSocketMessageType.Text, true, token ?? CancellationToken.None);
    }
    public async Task SendAsync(ReadOnlyMemory<byte> buffer, CancellationToken? token = null)
    {
        if (!started || client!.State != WebSocketState.Open)
            throw new InvalidOperationException("WebSocket is not open");

        await client.SendAsync(buffer, WebSocketMessageType.Binary, true, token ?? CancellationToken.None);
    }

    public async Task StopAsync()
    {
        if (!started)
            return;

        await client!.CloseAsync(WebSocketCloseStatus.NormalClosure, "Close", CancellationToken.None);
    }

    public void Dispose()
    {
        client?.Dispose();
        startLocker?.Dispose();
        GC.SuppressFinalize(this);
    }
}
