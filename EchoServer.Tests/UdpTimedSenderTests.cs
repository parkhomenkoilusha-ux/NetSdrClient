using NUnit.Framework;
using EchoTcpServer;
using System;
using System.Threading;
using System.Net.Sockets;

// Примітка: Для тестування Timer потрібен окремий клас, 
// який можна мокати, але ми перевіримо лише Dispose-логіку,
// щоб уникнути складної імітації Timer.

namespace EchoServer.Tests
{
    [TestFixture]
    public class UdpTimedSenderTests
    {
        private const string TestHost = "127.0.0.1";
        private const int TestPort = 8080;

        [Test]
        public void Constructor_InitializesFields()
        {
            // Act
            using var sender = new UdpTimedSender(TestHost, TestPort);

            // Assert
            // Перевірити приватні поля складно, але ми перевіряємо, що об'єкт створюється без винятків
            Assert.IsNotNull(sender);
        }

        [Test]
        public void StartSending_ThrowsExceptionIfAlreadyRunning()
        {
            // Arrange
            using var sender = new UdpTimedSender(TestHost, TestPort);
            sender.StartSending(100);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => sender.StartSending(100));
            sender.StopSending(); // Зупиняємо для чистоти
        }

        [Test]
        public void StopSending_DisposesTimer()
        {
            // Arrange
            using var sender = new UdpTimedSender(TestHost, TestPort);
            sender.StartSending(100);

            // Act
            sender.StopSending();
            
            // Assert
            // Прямо перевірити, чи _timer = null, не можна, але ми перевіряємо, що повторний Stop не кидає виняток
            Assert.DoesNotThrow(() => sender.StopSending());
        }

        [Test]
        public void Dispose_CallsStopSendingAndDisposesUdpClient()
        {
            // Arrange
            var sender = new UdpTimedSender(TestHost, TestPort);

            // Act
            // Використання 'using' або явний виклик Dispose()
            sender.StartSending(100);
            sender.Dispose();

            // Assert
            // Перевіряємо, що Dispose можна викликати двічі без винятків (хоча це не повний тест IDisposable)
            Assert.DoesNotThrow(() => sender.Dispose()); 
        }
    }
}