using Xunit;
using EchoTcpServer; // Підключаємо наш основний проект
using System.Text;
using System;

namespace EchoServer.Tests
{
    public class EchoHandlerTests
    {
        [Fact]
        public void Process_ShouldReturnSameData()
        {
            // Arrange (Підготовка)
            var handler = new EchoMessageHandler();
            string text = "Hello Lab 6";
            byte[] input = Encoding.UTF8.GetBytes(text);

            // Act (Дія)
            // Імітуємо, що прийшов пакет байтів
            byte[] result = handler.Process(input, input.Length);
            string resultText = Encoding.UTF8.GetString(result);

            // Assert (Перевірка)
            Assert.Equal(text, resultText); // Перевіряємо, що повернулося те саме
        }

        [Fact]
        public void Process_ShouldHandleEmptyInput()
        {
            // Arrange
            var handler = new EchoMessageHandler();
            byte[] input = new byte[1024];

            // Act
            byte[] result = handler.Process(input, 0); // 0 байт прочитано

            // Assert
            Assert.Empty(result); // Має повернути пустий масив
        }
    }
}