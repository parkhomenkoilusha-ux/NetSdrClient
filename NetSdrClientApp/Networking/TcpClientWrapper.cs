using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace NetSdrClientApp.Networking
{
    public class TcpClientWrapper : ITcpClient, IDisposable
    {
        private readonly string _host;
        private readonly int _port;
        private TcpClient? _client;
        private NetworkStream? _stream;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        public event EventHandler<byte[]>? MessageReceived;
        public bool Connected => _client?.Connected ?? false;

        public TcpClientWrapper(string host, int port)
        {
            _host = host;
            _port = port;
        }

        public void Connect()
        {
            if (Connected) return;
            _client = new TcpClient();
            _client.Connect(_host, _port);
            _stream = _client.GetStream();
            
            _ = Task.Run(() => ReceiveLoop(_cts.Token));
        }

        public void Disconnect()
        {
            _client?.Close();
            _client = null;
        }

        public async Task SendMessageAsync(byte[] message)
        {
            if (_stream != null)
                await _stream.WriteAsync(message, 0, message.Length);
        }

        private async Task ReceiveLoop(CancellationToken token)
        {
            try
            {
                byte[] buffer = new byte[8192];
                while (!token.IsCancellationRequested && _stream != null)
                {
                    int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length, token);
                    if (bytesRead == 0) break;
                    
                    byte[] data = new byte[bytesRead];
                    Array.Copy(buffer, data, bytesRead);
                    MessageReceived?.Invoke(this, data);
                }
            }
            catch
            {
                // Ignore connection errors on shutdown
            }
        }

        public void Dispose()
        {
            _cts.Cancel();
            _cts.Dispose();
            _stream?.Dispose();
            _client?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}