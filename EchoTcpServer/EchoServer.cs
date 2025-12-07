using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace EchoTcpServer
{
    public class EchoServer
    {
        private readonly int _port;
        private TcpListener? _listener; // FIX: Зроблено nullable
        private readonly CancellationTokenSource _cancellationTokenSource;
        
        private readonly IMessageHandler _messageHandler;

        public EchoServer(int port, IMessageHandler messageHandler)
        {
            _port = port;
            _messageHandler = messageHandler;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public async Task StartAsync()
        {
            _listener = new TcpListener(IPAddress.Any, _port);
            _listener.Start();
            Console.WriteLine($"Server started on port {_port}.");

            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    TcpClient client = await _listener.AcceptTcpClientAsync();
                    Console.WriteLine("Client connected.");

                    _ = Task.Run(() => HandleClientAsync(client, _cancellationTokenSource.Token));
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
            }

            Console.WriteLine("Server shutdown.");
        }

        private async Task HandleClientAsync(TcpClient client, CancellationToken token)
        {
            using (NetworkStream stream = client.GetStream())
            {
                try
                {
                    Memory<byte> buffer = new byte[8192]; // FIX: Використання Memory<byte>
                    int bytesRead;

                    // FIX: ReadAsync з Memory<byte>
                    while (!token.IsCancellationRequested && (bytesRead = await stream.ReadAsync(buffer, token)) > 0)
                    {
                        // Оскільки Process вимагає byte[], використовуємо Slice
                        byte[] response = _messageHandler.Process(buffer.Slice(0, bytesRead).ToArray(), bytesRead);

                        if (response.Length > 0)
                        {
                            // FIX: WriteAsync з ReadOnlyMemory<byte>
                            await stream.WriteAsync(response, token); 

                            // ✅ РЕФАКТОРИНГ: Видалено дублюючий Console.WriteLine
                            ReportBytesSent(response.Length); 
                        }
                    }
                }
                catch (Exception ex) when (!(ex is OperationCanceledException))
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
                finally
                {
                    client.Close();
                    Console.WriteLine("Client disconnected.");
                }
            }
        }
        
        // FIX: Зроблено статичним, оскільки не використовує поля класу
        private static void ReportBytesSent(int bytesCount)
        {
            if (bytesCount > 0)
            {
                Console.WriteLine($"Echoed {bytesCount} bytes to the client.");
            }
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
            _listener?.Stop(); // FIX: Safe access
            _cancellationTokenSource.Dispose();
            Console.WriteLine("Server stopped.");
        }
    }
}