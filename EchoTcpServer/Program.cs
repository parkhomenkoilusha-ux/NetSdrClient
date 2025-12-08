using System;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis; // <--- 1. Добавлено пространство имен

namespace EchoTcpServer
{
    [ExcludeFromCodeCoverage] // <--- 2. Добавлен атрибут исключения из покрытия
    class Program
    {
        public static async Task Main(string[] args)
        {
            // 1. Створюємо реалізацію нашої бізнес-логіки
            IMessageHandler handler = new EchoMessageHandler();

            // 2. Передаємо цю логіку в сервер
            EchoServer server = new EchoServer(5000, handler);

            // Start the server in a separate task
            var serverTask = server.StartAsync();

            string host = "127.0.0.1"; // Target IP
            int port = 60000;          // Target Port
            int intervalMilliseconds = 5000; // Send every 5 seconds

            using (var sender = new UdpTimedSender(host, port))
            {
                Console.WriteLine("Press any key to stop sending...");
                sender.StartSending(intervalMilliseconds);

                Console.WriteLine("Press 'q' to quit...");
                while (Console.ReadKey(intercept: true).Key != ConsoleKey.Q)
                {
                    // Just wait until 'q' is pressed
                }

                sender.StopSending();
                server.Stop();
                Console.WriteLine("Sender stopped.");
            }

            try
            {
                await serverTask;
            }
            catch 
            {
                // Ігноруємо помилки при завершенні
            }
        }
    }
}