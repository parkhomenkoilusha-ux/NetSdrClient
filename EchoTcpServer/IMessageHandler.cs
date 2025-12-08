using System;

namespace EchoTcpServer
{
    public interface IMessageHandler
    {
        // Приймає буфер і кількість байтів. Повертає відповідь.
        byte[] Process(byte[] buffer, int count);
    }
}