using NUnit.Framework;
using NetSdrClientApp.Networking;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NetSdrClientAppTests
{
    [TestFixture]
    public class UdpClientWrapperTests
    {
        // 1. Тестируем Equals и GetHashCode (это даст хороший прирост покрытия)
        [Test]
        public void Equals_And_HashCode_Should_Work_Correctly()
        {
            var wrapper1 = new UdpClientWrapper(12345);
            var wrapper2 = new UdpClientWrapper(12345);
            var wrapper3 = new UdpClientWrapper(54321);

            // Проверка Equals
            Assert.IsTrue(wrapper1.Equals(wrapper2)); // Одинаковые порты
            Assert.IsFalse(wrapper1.Equals(wrapper3)); // Разные порты
            Assert.IsFalse(wrapper1.Equals(null));     // null
            Assert.IsFalse(wrapper1.Equals(new object())); // Другой тип объекта

            // Проверка GetHashCode
            Assert.AreEqual(wrapper1.GetHashCode(), wrapper2.GetHashCode());
            Assert.AreNotEqual(wrapper1.GetHashCode(), wrapper3.GetHashCode());
        }

        // 2. Тестируем прием сообщений и остановку (покрывает StartListeningAsync, цикл и StopListening)
        [Test]
        public async Task StartListening_Should_Receive_Message_And_Stop_Gracefully()
        {
            // Arrange
            int port = 45678; // Используем свободный порт
            using var wrapper = new UdpClientWrapper(port);
            
            bool messageReceived = false;
            byte[] receivedData = Array.Empty<byte>();

            // Подписываемся на событие
            wrapper.MessageReceived += (sender, data) => 
            {
                messageReceived = true;
                receivedData = data;
            };

            // Act
            // 1. Запускаем прослушивание в отдельной задаче
            var listenTask = wrapper.StartListeningAsync();

            // 2. Отправляем реальный UDP пакет на этот порт
            using (var client = new UdpClient())
            {
                byte[] bytesToSend = { 0xAA, 0xBB, 0xCC };
                await client.SendAsync(bytesToSend, bytesToSend.Length, "127.0.0.1", port);
            }

            // 3. Ждем немного, чтобы сообщение успело дойти и обработаться
            await Task.Delay(200);

            // 4. Останавливаем (это должно вызвать ObjectDisposedException внутри, который ловится в catch)
            wrapper.StopListening();
            
            // Ждем завершения задачи (чтобы убедиться, что catch сработал)
            await listenTask;

            // Assert
            Assert.IsTrue(messageReceived, "Событие MessageReceived не сработало");
            Assert.AreEqual(0xAA, receivedData[0]);
            Assert.AreEqual(0xCC, receivedData[2]);
        }

        // 3. Тестируем Dispose отдельно
        [Test]
        public void Dispose_Should_Not_Throw()
        {
            var wrapper = new UdpClientWrapper(45679);
            Assert.DoesNotThrow(() => wrapper.Dispose());
            // Повторный вызов тоже не должен падать
            Assert.DoesNotThrow(() => wrapper.Dispose());
        }
    }
}