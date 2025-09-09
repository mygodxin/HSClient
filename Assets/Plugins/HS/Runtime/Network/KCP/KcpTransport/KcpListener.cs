using KcpTransport.LowLevel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace KcpTransport
{

    public abstract class KcpOptions
    {
        public bool EnableNoDelay { get; set; } = true;
        public int IntervalMilliseconds { get; set; } = 10; // ikcp_nodelay min is 10.
        public int Resend { get; set; } = 2;
        public bool EnableFlowControl { get; set; } = false;
        public (int SendWindow, int ReceiveWindow) WindowSize { get; set; } = ((int)KcpMethods.IKCP_WND_SND, (int)KcpMethods.IKCP_WND_RCV);
        public int MaximumTransmissionUnit { get; set; } = (int)KcpMethods.IKCP_MTU_DEF;
        // public int MinimumRetransmissionTimeout { get; set; } this value is changed in ikcp_nodelay(and use there default) so no configurable.
    }

    public enum ListenerSocketType
    {
        Receive,
        Send
    }

    public delegate ReadOnlySpan<byte> HashFunc();

    public sealed class KcpListenerOptions : KcpOptions
    {
        //static readonly byte[] DefaultRandomHashKey = new byte[32];

        public IPEndPoint ListenEndPoint { get; set; }
        public TimeSpan UpdatePeriod { get; set; } = TimeSpan.FromMilliseconds(5);
        public int EventLoopCount { get; set; } = Math.Max(1, Environment.ProcessorCount / 2);
        public bool ConfigureAwait { get; set; } = false;
        public TimeSpan KeepAliveDelay { get; set; } = TimeSpan.FromSeconds(20);
        public TimeSpan ConnectionTimeout { get; set; } = TimeSpan.FromMinutes(1);
        public TimeSpan HandshakeTimeout { get; set; } = TimeSpan.FromSeconds(30);
        public HashFunc Handshake32bitHashKeyGenerator { get; set; } = KeyGenerator;
        public Action<Socket, KcpListenerOptions, ListenerSocketType>? ConfigureSocket { get; set; }

        static ReadOnlySpan<byte> KeyGenerator() => DefaultRandomHashKey;

        private static byte[] _defaultRandomHashKey;
        private static readonly object _lock = new object();
        public static byte[] DefaultRandomHashKey
        {
            get
            {
                if (_defaultRandomHashKey == null)
                {
                    lock (_lock)
                    {
                        if (_defaultRandomHashKey == null)
                        {
                            byte[] buffer = new byte[32];
                            using (var rng = new RNGCryptoServiceProvider())
                            {
                                rng.GetBytes(buffer);
                            }
                            _defaultRandomHashKey = buffer;
                        }
                    }
                }
                return _defaultRandomHashKey;
            }
        }
    }

    public sealed class KcpListener : IDisposable, IAsyncDisposable
    {
        Socket socket;
        ConcurrentQueue<KcpConnection> acceptQueue;
        bool isDisposed;
        ConcurrentDictionary<uint, KcpConnection> connections = new();

        Task[] socketEventLoopTasks;
        Thread updateConnectionsWorkerThread;
        CancellationTokenSource listenerCancellationTokenSource = new();

        public static ValueTask<KcpListener> ListenAsync(string host, int port, CancellationToken cancellationToken = default)
        {
            return ListenAsync(new IPEndPoint(IPAddress.Parse(host), port), cancellationToken);
        }

        public static ValueTask<KcpListener> ListenAsync(IPEndPoint listenEndPoint, CancellationToken cancellationToken = default)
        {
            return ListenAsync(new KcpListenerOptions { ListenEndPoint = listenEndPoint }, cancellationToken);
        }

        public static ValueTask<KcpListener> ListenAsync(KcpListenerOptions options, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // sync but for future extensibility.
            return new ValueTask<KcpListener>(new KcpListener(options));
        }

        KcpListener(KcpListenerOptions options)
        {
            Socket socket = new Socket(options.ListenEndPoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            socket.Blocking = false;
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true); // in Linux, as SO_REUSEPORT

            // http://handyresearcher.blog.fc2.com/blog-entry-18.html
            // https://stackoverflow.com/questions/7201862/an-existing-connection-was-forcibly-closed-by-the-remote-host/7478498
            // https://stackoverflow.com/questions/34242622/windows-udp-sockets-recvfrom-fails-with-error-10054
            // https://stackoverflow.com/questions/74327225/why-does-sending-via-a-udpclient-cause-subsequent-receiving-to-fail

            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
      System.Runtime.InteropServices.OSPlatform.Windows))
            {
                const uint IOC_IN = 0x80000000U;
                const uint IOC_VENDOR = 0x18000000U;
                const uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
                socket.IOControl(unchecked((int)SIO_UDP_CONNRESET), new byte[] { 0x00, 0x00, 0x00, 0x00 }, null);
            }

            options.ConfigureSocket?.Invoke(socket, options, ListenerSocketType.Receive);

            var endPoint = options.ListenEndPoint;
            try
            {
                socket.Bind(endPoint);
            }
            catch
            {
                socket.Dispose();
                throw;
            }

            this.socket = socket;
            this.acceptQueue = new ConcurrentQueue<KcpConnection>();

            this.socketEventLoopTasks = new Task[options.EventLoopCount];
            for (int i = 0; i < socketEventLoopTasks.Length; i++)
            {
                socketEventLoopTasks[i] = this.StartSocketEventLoopAsync(options, i);
            }

            updateConnectionsWorkerThread = new Thread(RunUpdateKcpConnectionLoop)
            {
                Name = $"{nameof(KcpListener)}",
                IsBackground = true,
                Priority = ThreadPriority.AboveNormal,
            };
            updateConnectionsWorkerThread.Start(options);
        }

        public ValueTask<KcpConnection> AcceptConnectionAsync(CancellationToken cancellationToken = default)
        {
            // 同步完成：队列中有可用连接
            acceptQueue.TryDequeue(out var connection);
            return new ValueTask<KcpConnection>(connection); // 包装 Task
        }

        async Task StartSocketEventLoopAsync(KcpListenerOptions options, int id)
        {
            // await Task.CompletedTask.ConfigureAwait(ConfigureAwaitOptions.ForceYielding);

            var remoteEndPoint = options.ListenEndPoint;
            var random = new Random();
            var cancellationToken = this.listenerCancellationTokenSource.Token;

            var socketBuffer = new byte[options.MaximumTransmissionUnit];

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // Socket is datagram so received data contains full block
                    var result = await socket.ReceiveFromAsync(socketBuffer, SocketFlags.None, remoteEndPoint);
                    var received = result.ReceivedBytes;
                    remoteEndPoint = (IPEndPoint)result.RemoteEndPoint;
                    // first 4 byte is conversationId or extra packet type
                    var conversationId = MemoryMarshal.Read<uint>(socketBuffer.AsSpan(0, received));
                    var packetType = (PacketType)conversationId;
                    switch (packetType)
                    {
                        case PacketType.HandshakeInitialRequest:
                        ISSUE_CONVERSATION_ID:
                            {
                                conversationId = unchecked((uint)random.Next(100, int.MaxValue)); // 0~99 is reserved
                                if (connections.ContainsKey(conversationId))
                                {
                                    goto ISSUE_CONVERSATION_ID;
                                }

                                // send tentative id and cookie to avoid syn-flood(don't allocate memory in this phase)
                                var (cookie, timestamp) = SynCookie.Generate(options.Handshake32bitHashKeyGenerator(), remoteEndPoint);

                                SendHandshakeInitialResponse(socket, remoteEndPoint, conversationId, cookie, timestamp);
                            }
                            break;
                        case PacketType.HandshakeOkRequest:
                            {
                                conversationId = MemoryMarshal.Read<uint>(socketBuffer.AsSpan(4, received - 4));
                                var cookie = MemoryMarshal.Read<uint>(socketBuffer.AsSpan(8, received - 8));
                                var timestamp = MemoryMarshal.Read<long>(socketBuffer.AsSpan(12, received - 12));

                                if (!SynCookie.Validate(options.Handshake32bitHashKeyGenerator(), options.HandshakeTimeout, cookie, remoteEndPoint, timestamp))
                                {
                                    SendHandshakeNgResponse(socket, remoteEndPoint);
                                    break;
                                }

                                if (!connections.TryAdd(conversationId, null!))
                                {
                                    // can't added, client should retry
                                    SendHandshakeNgResponse(socket, remoteEndPoint);
                                    break;
                                }

                                // create new connection
                                var kcpConnection = new KcpConnection(conversationId, options, remoteEndPoint);
                                connections[conversationId] = kcpConnection;
                                acceptQueue.Enqueue(kcpConnection);
                                SendHandshakeOkResponse(socket, remoteEndPoint);
                            }
                            break;
                        case PacketType.Ping:
                            {
                                conversationId = MemoryMarshal.Read<uint>(socketBuffer.AsSpan(4, received - 4));
                                if (!connections.TryGetValue(conversationId, out var kcpConnection))
                                {
                                    // may incoming old packet, TODO: log it.
                                    continue;
                                }

                                kcpConnection.PingReceived();
                            }
                            break;
                        case PacketType.Pong:
                            {
                                conversationId = MemoryMarshal.Read<uint>(socketBuffer.AsSpan(4, received - 4));
                                if (!connections.TryGetValue(conversationId, out var kcpConnection))
                                {
                                    // may incoming old packet, TODO: log it.
                                    continue;
                                }

                                kcpConnection.PongReceived();
                            }
                            break;
                        case PacketType.Disconnect:
                            {
                                conversationId = MemoryMarshal.Read<uint>(socketBuffer.AsSpan(4, received - 4));
                                if (!connections.TryRemove(conversationId, out var kcpConnection))
                                {
                                    // may incoming old packet, TODO: log it.
                                    continue;
                                }

                                kcpConnection.Disconnect();
                                kcpConnection.Dispose();
                            }
                            break;
                        case PacketType.Unreliable:
                            {
                                conversationId = MemoryMarshal.Read<uint>(socketBuffer.AsSpan(4, received - 4));
                                if (!connections.TryGetValue(conversationId, out var kcpConnection))
                                {
                                    // may incoming old packet, TODO: log it.
                                    continue;
                                }


                                // This loop is sometimes called in multithread so needs lock per connection.
                                lock (kcpConnection.SyncRoot)
                                {
                                    kcpConnection.InputReceivedUnreliableBuffer(socketBuffer.AsSpan(8, received - 8));
                                }
                            }
                            break;
                        default:
                            {
                                // Reliable
                                if (conversationId < 100)
                                {
                                    // may incoming invalid packet, TODO: log it.
                                    continue;
                                }
                                if (!connections.TryGetValue(conversationId, out var kcpConnection))
                                {
                                    // may incoming old packet, TODO: log it.
                                    continue;
                                }

                                lock (kcpConnection.SyncRoot)
                                {
                                    unsafe
                                    {
                                        fixed (byte* socketBufferPointer = socketBuffer)
                                        {
                                            if (!kcpConnection.InputReceivedKcpBuffer(socketBufferPointer, received)) continue;
                                        }
                                    }

                                    kcpConnection.ConsumeKcpFragments(remoteEndPoint, cancellationToken);
                                }
                            }
                            break;
                    }
                }
                catch (Exception ex)
                {
                    // TODO: log?
                    _ = ex;
                }
            }

            static void SendHandshakeInitialResponse(Socket socket, IPEndPoint clientAddress, uint conversationId, uint cookie, long timestamp)
            {
                byte[] data = new byte[20];
                Buffer.BlockCopy(BitConverter.GetBytes((uint)PacketType.HandshakeInitialResponse), 0, data, 0, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(conversationId), 0, data, 4, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(cookie), 0, data, 8, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(timestamp), 0, data, 12, 8);

                socket.SendTo(data, SocketFlags.None, clientAddress);
            }

            static void SendHandshakeOkResponse(Socket socket, IPEndPoint clientAddress)
            {
                var data = new byte[4];
                var msgId = (uint)PacketType.HandshakeOkResponse;
                MemoryMarshal.Write(data, ref msgId);
                socket.SendTo(data, SocketFlags.None, clientAddress);
            }

            static void SendHandshakeNgResponse(Socket socket, IPEndPoint clientAddress)
            {
                var data = new byte[4];
                var msgId = (uint)PacketType.HandshakeNgResponse;
                MemoryMarshal.Write(data, ref msgId);
                socket.SendTo(data, SocketFlags.None, clientAddress);
            }
        }

        void RunUpdateKcpConnectionLoop(object? state)
        {
            // NOTE: should use ikcp_check? https://github.com/skywind3000/kcp/wiki/EN_KCP-Best-Practice#advance-update

            // All Windows(.NET) Timer and Sleep is low-resolution(min is 16ms).
            // We use custom high-resolution timer instead.

            var cancellationToken = listenerCancellationTokenSource.Token;
            var options = (KcpListenerOptions)state!;
            var period = options.UpdatePeriod;
            var timeout = options.ConnectionTimeout;
            var waitTime = (int)period.TotalMilliseconds;

            var removeConnection = new List<uint>();
            while (true)
            {
                Thread.Sleep(waitTime);

                if (cancellationToken.IsCancellationRequested) break;

                var currentTimestamp = Stopwatch.GetTimestamp();
                foreach (var kvp in connections)
                {
                    var connection = kvp.Value;
                    if (connection != null) // at handshake, connection is not created yet so sometimes null...
                    {
                        if (connection.IsAlive(currentTimestamp, timeout))
                        {
                            connection.TrySendPing(currentTimestamp);
                            connection.UpdateKcp();
                        }
                        else
                        {
                            removeConnection.Add(kvp.Key);
                        }
                    }
                }

                if (removeConnection.Count > 0)
                {
                    foreach (var id in removeConnection)
                    {
                        if (connections.TryRemove(id, out var conn))
                        {
                            conn.Disconnect();
                            conn.Dispose();
                        }
                    }

                    removeConnection.Clear();
                }
            }
        }

        public unsafe void Dispose()
        {
            DisposeAsync().GetAwaiter().GetResult();
        }

        public async ValueTask DisposeAsync()
        {
            if (isDisposed) return;
            isDisposed = true;

            acceptQueue.Clear();

            // loop will stop
            listenerCancellationTokenSource.Cancel();
            listenerCancellationTokenSource.Dispose();

            // wait loop complete
            try
            {
                var curr = SynchronizationContext.Current;
                await Task.WhenAll(socketEventLoopTasks);
            }
            catch { }

            // before socket dispose, send disocnnected event to all connections
            foreach (var connection in connections)
            {
                var conn = connection.Value;
                if (conn != null)
                {
                    conn.Disconnect();
                    conn.Dispose();
                }
            }
            connections.Clear();

            socket.Dispose();
        }
    }
}
