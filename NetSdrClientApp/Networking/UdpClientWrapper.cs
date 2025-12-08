using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace NetSdrClientApp.Networking
{
    public class UdpClientWrapper : IUdpClient, IDisposable
    {
        private readonly int _listenPort;
        private UdpClient? _udpClient;
        // FIX: Blocker - цей ресурс треба звільняти
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        public event EventHandler<byte[]>? MessageReceived;

        public UdpClientWrapper(int listenPort)
        {
            _listenPort = listenPort;
        }

        public async Task StartListeningAsync()
        {
            _udpClient = new UdpClient(_listenPort);
            try
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    // Використовуємо ReceiveAsync, що підтримує токен (у новіших .NET)
                    // або просто ReceiveAsync() і перериваємо через Close()
                    var result = await _udpClient.ReceiveAsync();
                    MessageReceived?.Invoke(this, result.Buffer);
                }
            }
            catch (ObjectDisposedException) { }
            catch (Exception) { /* Ignore socket errors on shutdown */ }
        }

        public void StopListening()
        {
            if (!_cts.IsCancellationRequested)
            {
                _cts.Cancel();
            }
            _udpClient?.Close();
        }

        // FIX: Major - Якщо перевизначено GetHashCode, треба і Equals
        public override int GetHashCode()
        {
            return _listenPort.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            if (obj is UdpClientWrapper other)
            {
                return _listenPort == other._listenPort;
            }
            return false;
        }

        // FIX: Blocker - Реалізація Dispose для звільнення _cts
        public void Dispose()
        {
            StopListening();
            _cts.Dispose();
            _udpClient?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}