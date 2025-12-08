using NUnit.Framework;
using NetSdrClientApp.Messages;
using System;
using System.Linq;

namespace NetSdrClientAppTests
{
    [TestFixture]
    public class NetSdrMessageHelperTests
    {
        // 1. Тест создания Control Message
        [Test]
        public void GetControlItemMessage_Should_Create_Correct_Byte_Array()
        {
            // Arrange
            var type = NetSdrMessageHelper.MsgTypes.SetControlItem; // Value 4
            var code = NetSdrMessageHelper.ControlItemCodes.ReceiverFrequency; // Value 0x0020
            byte[] parameters = { 0x01, 0x02 };

            // Act
            byte[] result = NetSdrMessageHelper.GetControlItemMessage(type, code, parameters);

            // Assert
            // Length = 2 (header) + 2 (code) + 2 (params) = 6
            Assert.AreEqual(6, result.Length);
            
            // Header: (4 << 13) | 6 = 32768 | 6 = 32774 -> [0x06, 0x80] (Little Endian)
            ushort expectedHeader = (ushort)((ushort)type << 13 | 6);
            Assert.AreEqual(BitConverter.GetBytes(expectedHeader)[0], result[0]);
            
            // Code at index 2
            Assert.AreEqual(0x20, result[2]); 
            
            // Params at index 4
            Assert.AreEqual(0x01, result[4]);
        }

        // 2. Тест создания Data Message
        [Test]
        public void GetDataItemMessage_Should_Create_Correct_Byte_Array()
        {
            // Arrange
            var type = NetSdrMessageHelper.MsgTypes.DataItem;
            byte[] parameters = { 0xFF };

            // Act
            byte[] result = NetSdrMessageHelper.GetDataItemMessage(type, parameters);

            // Assert
            // Length = 2 (header) + 1 (param) = 3
            Assert.AreEqual(3, result.Length);
            Assert.AreEqual(0xFF, result[2]);
        }

        // 3. Тест TranslateMessage: NULL или короткое сообщение (Guard Clause)
        [Test]
        public void TranslateMessage_Should_Handle_Null_Or_Empty()
        {
            NetSdrMessageHelper.TranslateMessage(null, out var type, out var code, out var seq, out var body);
            Assert.AreEqual(NetSdrMessageHelper.MsgTypes.Nack, type);

            NetSdrMessageHelper.TranslateMessage(new byte[1], out type, out code, out seq, out body);
            Assert.AreEqual(NetSdrMessageHelper.MsgTypes.Nack, type);
        }

        // 4. Тест TranslateMessage: Control Type (Ветка IF)
        [Test]
        public void TranslateMessage_Should_Parse_Control_Message()
        {
            // Создаем сообщение через сам хелпер, чтобы быть уверенным в формате
            var msg = NetSdrMessageHelper.GetControlItemMessage(
                NetSdrMessageHelper.MsgTypes.Ack, 
                NetSdrMessageHelper.ControlItemCodes.RFFilter, 
                new byte[] { 0xAA });

            // Act
            NetSdrMessageHelper.TranslateMessage(msg, out var type, out var code, out var seq, out var body);

            // Assert
            Assert.AreEqual(NetSdrMessageHelper.MsgTypes.Ack, type);
            Assert.AreEqual(NetSdrMessageHelper.ControlItemCodes.RFFilter, code);
            Assert.AreEqual(0xAA, body[0]);
        }

        // 5. Тест TranslateMessage: Data Type (Ветка ELSE)
        [Test]
        public void TranslateMessage_Should_Parse_Data_Message()
        {
            // Создаем сообщение типа DataItem (оно попадет в else в TranslateMessage)
            var msg = NetSdrMessageHelper.GetDataItemMessage(
                NetSdrMessageHelper.MsgTypes.DataItem, 
                new byte[] { 0xBB, 0xCC });

            // Act
            NetSdrMessageHelper.TranslateMessage(msg, out var type, out var code, out var seq, out var body);

            // Assert
            Assert.AreEqual(NetSdrMessageHelper.MsgTypes.DataItem, type);
            Assert.AreEqual(0, (int)code); // Код должен быть 0 для DataItem
            Assert.AreEqual(2, body.Length);
            Assert.AreEqual(0xBB, body[0]);
        }

        // 6. Тест GetSamples: Успешная конвертация (Happy Path)
        [Test]
        public void GetSamples_Should_Convert_Bytes_To_Ints()
        {
            // 16 бит = 2 байта на сэмпл. 
            // Данные: [0x01, 0x00] -> число 1. [0x02, 0x00] -> число 2.
            ushort sampleSize = 16;
            byte[] body = { 0x01, 0x00, 0x02, 0x00 };

            var result = NetSdrMessageHelper.GetSamples(sampleSize, body).ToList();

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(1, result[0]);
            Assert.AreEqual(2, result[1]);
        }

        // 7. Тест GetSamples: Ошибка размера (Exception)
        // Это покроет CheckSamplesParameters throw new ArgumentOutOfRangeException
        [Test]
        public void GetSamples_Should_Throw_If_Size_Too_Big()
        {
            ushort tooBigSampleSize = 40; // 40 / 8 = 5 байт (больше 4)
            byte[] body = { 0x00 };

            Assert.Throws<ArgumentOutOfRangeException>(() => 
                NetSdrMessageHelper.GetSamples(tooBigSampleSize, body));
        }
    }
}