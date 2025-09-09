
using KcpTransport;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Net.KCP
{
    public class KcpServer : IDisposable
    {
        private KcpListener _transport;
        private readonly CancellationTokenSource _cts = new();
        private readonly IPEndPoint _localEndPoint;

        public KcpServer(string host, int port)
        {
            _localEndPoint = new IPEndPoint(IPAddress.Parse(host), port);
        }
        public KcpServer(IPEndPoint localEndPoint)
        {
            _localEndPoint = localEndPoint;
        }

        public async Task StartAsync()
        {
            _transport = await KcpListener.ListenAsync(_localEndPoint, _cts.Token);
            // 启动接收循环
            _ = Task.Run(ReceiveLoopAsync);
        }

        private async Task ReceiveLoopAsync()
        {
            var buffer = new byte[1024];
            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    var result = await _transport.AcceptConnectionAsync();
                    if (result == null) continue;
                    if (result.ConnectionId == 0) continue;

                    var socket = new KcpClient(result);
                    socket.OnReceived = ((data) =>
                    {
                        OnReceiveData(data, socket);
                    });
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Receive error: {ex}");
                }
            }
        }

        public void Dispose()
        {
            _cts.Cancel();
            _transport.Dispose();
            GC.SuppressFinalize(this);
        }

        private void OnReceiveData(byte[] buffer, KcpClient channel)
        {
            //MessageHandle.Read(buffer, channel);
        }
    }
}