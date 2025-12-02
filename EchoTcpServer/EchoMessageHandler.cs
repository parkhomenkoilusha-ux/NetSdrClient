using System;

namespace EchoTcpServer
{
    public class EchoMessageHandler : IMessageHandler
    {
        public byte[] Process(byte[] buffer, int count)
        {
            // Якщо нічого не прийшло — повертаємо пустий масив
            if (count <= 0) return Array.Empty<byte>();

            // Логіка Ехо: створюємо копію отриманих даних, щоб відправити назад
            byte[] response = new byte[count];
            Array.Copy(buffer, response, count);
            
            return response;
        }
    }
}