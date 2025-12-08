using NUnit.Framework;
using NetSdrClientApp;
using NetSdrClientApp.Networking;
using EchoTcpServer; // Нам нужно ссылаться на сервер
using System.Threading.Tasks;
using System.Threading;
using System;

namespace NetSdrClientAppTests
{
    [TestFixture]
    public class IntegrationTests
    {
        private EchoServer _server;
        private TcpClientWrapper _realTcp;
        private UdpClientWrapper _realUdp;
        private NetSdrClient _client;
        private const int TestPort = 5555; // Используем порт для тестов

        [SetUp]
        public void Setup()
        {
            // 1. Поднимаем РЕАЛЬНЫЙ сервер
            // Нам нужен любой IMessageHandler, используем заглушку или реальный, если доступен
            var handler = new EchoMessageHandler(); 
            _server = new EchoServer(TestPort, handler);
            
            // Запускаем сервер в фоне
            _ = _server.StartAsync();
            
            // Даем серверу немного времени на старт
            Thread.Sleep(200);

            // 2. Создаем РЕАЛЬНЫХ клиентов (не Mocks!)
            // Это покроет код внутри TcpClientWrapper и UdpClientWrapper
            _realTcp = new TcpClientWrapper("127.0.0.1", TestPort);
            _realUdp = new UdpClientWrapper(0); // 0 = любой свободный порт

            // 3. Создаем основного клиента
            _client = new NetSdrClient(_realTcp, _realUdp);
        }

        [TearDown]
        public void Teardown()
        {
            // Очистка ресурсов (это тоже добавит coverage в методы Dispose)
            _client?.Disconect();
            _realTcp?.Dispose();
            _realUdp?.Dispose();
            _server?.Stop();
        }

        [Test]
        public async Task FullSystem_ConnectAndStartIQ_IntegrationTest()
        {
            // Этот тест проходит по ВСЕМ реальным классам сразу

            // 1. Проверяем подключение
            await _client.ConnectAsync();
            Assert.IsTrue(_realTcp.Connected, "TCP Client should be connected to EchoServer");

            // 2. Проверяем запуск IQ (отправка команд на сервер)
            await _client.StartIQAsync();
            Assert.IsTrue(_client.IQStarted, "IQ should be started");

            // 3. Проверяем смену частоты
            await _client.ChangeFrequencyAsync(102000, 0);

            // 4. Проверяем остановку
            await _client.StopIQAsync();
            Assert.IsFalse(_client.IQStarted, "IQ should be stopped");
        }
    }
}