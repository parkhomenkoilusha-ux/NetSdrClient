using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace EchoTcpServer
{
    /// <summary>
    /// This program was designed for test purposes only
    /// Not for a review
    /// </summary>
    public class EchoServer
    {
        private readonly int _port;
        private TcpListener _listener;
        private readonly CancellationTokenSource _cancellationTokenSource;
        
        // Додаємо поле для нашого обробника
        private readonly IMessageHandler _messageHandler;

        // Змінюємо конструктор: тепер він вимагає IMessageHandler
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
                    // Listener has been closed
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
                    byte[] buffer = new byte[8192];
                    int bytesRead;

                    while (!token.IsCancellationRequested && (bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, token)) > 0)
                    {
                        // ВАЖЛИВО: Замість прямого echo, ми викликаємо наш handler
                        // Це дозволяє нам тестувати логіку окремо від мережі
                        byte[] response = _messageHandler.Process(buffer, bytesRead);

                        // Якщо є що відправляти - відправляємо
                        if (response.Length > 0)
                        {
                            await stream.WriteAsync(response, 0, response.Length, token);
                            Console.WriteLine($"Echoed {response.Length} bytes to the client.");
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

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
            _listener.Stop();
            _cancellationTokenSource.Dispose();
            Console.WriteLine("Server stopped.");
        }
    }
}