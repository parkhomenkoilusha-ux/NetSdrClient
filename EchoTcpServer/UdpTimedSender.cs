using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace EchoTcpServer
{
    public class UdpTimedSender : IDisposable // Вимагає реалізації IDisposable
    {
        private readonly string _host;
        private readonly int _port;
        private readonly UdpClient _udpClient;
        private Timer? _timer; // FIX: Зроблено nullable
        private bool _disposed = false; // Поле для відстеження стану Dispose

        public UdpTimedSender(string host, int port)
        {
            _host = host;
            _port = port;
            _udpClient = new UdpClient();
        }

        public void StartSending(int intervalMilliseconds)
        {
            if (_timer != null)
                throw new InvalidOperationException("Sender is already running.");

            // FIX: TimerCallback підтримує object?
            _timer = new Timer(SendMessageCallback, null, 0, intervalMilliseconds);
        }

        private ushort i = 0;

        // FIX: Параметр зроблено nullable
        private void SendMessageCallback(object? state)
        {
            try
            {
                // ... (original logic) ...
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message: {ex.Message}");
            }
        }

        public void StopSending()
        {
            _timer?.Dispose();
            _timer = null;
        }

        // FIX: Реалізація IDisposable Pattern
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this); // FIX: Додано GC.SuppressFinalize
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    StopSending(); // Викликаємо для коректної зупинки і Dispose таймера
                    _udpClient.Dispose();
                }

                _disposed = true;
            }
        }
    }
}