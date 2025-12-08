using NUnit.Framework; // Используем NUnit вместо Xunit
using EchoTcpServer;
using System;

namespace NetSdrClientAppTests // Пространство имен должно совпадать с вашим проектом тестов
{
    [TestFixture]
    public class UdpTimedSenderTests
    {
        [Test]
        public void Dispose_Should_Cover_StopSending_And_ClientDispose()
        {
            // Arrange
            string host = "127.0.0.1";
            int port = 12345;
            var sender = new UdpTimedSender(host, port);

            sender.StartSending(500);

            // Act
            sender.Dispose();

            // Assert
            // В NUnit проверяем, что код не выбрасывает ошибок при повторном вызове
            Assert.DoesNotThrow(() => sender.Dispose());
        }

        [Test]
        public void StartSending_Should_Throw_If_Already_Running()
        {
            var sender = new UdpTimedSender("127.0.0.1", 12345);
            sender.StartSending(100);

            // Синтаксис NUnit для проверки исключений
            Assert.Throws<InvalidOperationException>(() => sender.StartSending(100));

            sender.Dispose();
        }

        [Test]
        public void StopSending_Should_Dispose_Timer()
        {
            var sender = new UdpTimedSender("127.0.0.1", 12345);
            sender.StartSending(100);

            sender.StopSending();

            // Повторный запуск должен сработать без ошибок
            Assert.DoesNotThrow(() => sender.StartSending(100));

            sender.Dispose();
        }
    }
}