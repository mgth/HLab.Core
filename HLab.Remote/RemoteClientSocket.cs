#nullable enable
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HLab.Remote;

public class RemoteClientSocket(string hostname, int port) : IRemoteClient
{
    public event EventHandler<string>? MessageReceived;
    public event EventHandler? Connected;
    public event EventHandler? ConnectionFailed;
    public bool Stopping { get; set; } = false;

    CancellationTokenSource _token = new();

    public void Listen()
    {
        Task.Run(ListenThread);
    }

    void ListenThread()
    {
        while (!Stopping)
        {
            TcpClient? client = null;
            //if client is not connected, try to connect
            while (client is not { Connected: true })
            {
                var wait = 50;
                while (!Stopping)
                {
                    try
                    {
                        if (client is not null)
                        {
                            client.Close();
                            client.Dispose();
                        }
                        client = new TcpClient(hostname, port);
                        if (client.Connected)
                        {
                            Connected?.Invoke(this, EventArgs.Empty);
                            break;
                        }
                    }
                    catch (SocketException)
                    {
                    }

                    try
                    {
                        ConnectionFailed?.Invoke(this, EventArgs.Empty);
                    }
                    catch (Exception ex)
                    {

                    }

                    if (wait > 0) Thread.Sleep(wait);
                    if (wait < 10000) wait *= 2;
                }
            }

            if (client is null) continue;

            var stream = client.GetStream();

            using (var writer = new StreamWriter(stream))
            using (var reader = new StreamReader(stream))
            {

                writer.AutoFlush = true;
                writer.WriteLine("""<CommandMessage Command="Listen" Payload=""/>""");

                while (client.Connected)
                {
                    try
                    {
                        var msg = reader.ReadLine();

                        if (msg != null)
                            _ = Task.Run(() => MessageReceived?.Invoke(this, msg));
                    }
                    catch (ObjectDisposedException)
                    {
                        break;
                    }
                    catch (SocketException)
                    {
                        break;
                    }
                    catch (IOException)
                    {
                        break;
                    }
                }

            }

            MessageReceived?.Invoke(this, "<DaemonMessage><State>Dead</State></DaemonMessage>\n");
        }
    }

    async Task<bool> TrySendMessageAsync(string message, CancellationToken token)
    {
        try
        {
            using (var client = new TcpClient(hostname, port))
            {
                await using (var w = new StreamWriter(client.GetStream()))
                {
                    w.AutoFlush = true;
                    var sb = new StringBuilder(message);

                    await w.WriteLineAsync(sb.ToString());
                    client.Close();
                };
            };
        }
        catch (Exception ex)
        {
            return false;
        }
        return true;
    }


    public async Task SendMessageAsync(string message, CancellationToken token)
    {
        var delay = 500;
        for (var i = 0; i < 10; i++)
        {
            try
            {
                if (await TrySendMessageAsync(message, token))
                    return;
            }
            catch (Exception ex)
            {
            }

            await Task.Delay(delay, token);
            delay *= 2;
        }
    }
}