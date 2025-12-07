using NUnit.Framework;
using NetSdrClientApp.Messages;
using System;
using System.Linq;
using System.Collections.Generic; // Необхідно для ToList()

namespace NetSdrClientAppTests
{
    [TestFixture]
    public class NetSdrMessageHelperTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void GetControlItemMessageTest()
        {
            //Arrange
            var type = NetSdrMessageHelper.MsgTypes.Ack;
            var code = NetSdrMessageHelper.ControlItemCodes.ReceiverState;
            int parametersLength = 7500;

            //Act
            byte[] msg = NetSdrMessageHelper.GetControlItemMessage(type, code, new byte[parametersLength]);

            var headerBytes = msg.Take(2);
            var codeBytes = msg.Skip(2).Take(2);
            var parametersBytes = msg.Skip(4);

            var num = BitConverter.ToUInt16(headerBytes.ToArray());
            var actualType = (NetSdrMessageHelper.MsgTypes)(num >> 13);
            var actualLength = num - ((int)actualType << 13);
            var actualCode = BitConverter.ToInt16(codeBytes.ToArray());

            //Assert
            Assert.That(headerBytes.Count(), Is.EqualTo(2));
            Assert.That(msg.Length, Is.EqualTo(actualLength));
            Assert.That(type, Is.EqualTo(actualType));

            Assert.That(actualCode, Is.EqualTo((short)code));

            Assert.That(parametersBytes.Count(), Is.EqualTo(parametersLength));
        }

        [Test]
        public void GetDataItemMessageTest()
        {
            //Arrange
            var type = NetSdrMessageHelper.MsgTypes.DataItem2;
            int parametersLength = 7500;

            //Act
            byte[] msg = NetSdrMessageHelper.GetDataItemMessage(type, new byte[parametersLength]);

            var headerBytes = msg.Take(2);
            var parametersBytes = msg.Skip(2);

            var num = BitConverter.ToUInt16(headerBytes.ToArray());
            var actualType = (NetSdrMessageHelper.MsgTypes)(num >> 13);
            var actualLength = num - ((int)actualType << 13);

            //Assert
            Assert.That(headerBytes.Count(), Is.EqualTo(2));
            Assert.That(msg.Length, Is.EqualTo(actualLength));
            Assert.That(type, Is.EqualTo(actualType));

            Assert.That(parametersBytes.Count(), Is.EqualTo(parametersLength));
        }

        // --- ДОДАТКОВІ ТЕСТИ ДЛЯ ПІДВИЩЕННЯ COVERAGE (Lab 8) ---
        
        [Test]
        public void GetSamples_ThrowsArgumentOutOfRangeException_ForInvalidSampleSize()
        {
            // Arrange
            ushort invalidSize = 40; // 5 bytes
            byte[] dummyBody = new byte[10];

            // Act & Assert
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => 
            {
                _ = NetSdrMessageHelper.GetSamples(invalidSize, dummyBody).ToList();
            });

            Assert.That(ex.ParamName, Is.EqualTo("sampleSize"));
            Assert.That(ex.Message, Does.Contain("Sample size must not exceed 32 bits (4 bytes)."));
        }

        [Test]
        public void GetSamples_ReturnsCorrectSamples_For16BitSamples()
        {
            // Arrange (Little Endian: 00 01 = 256; 00 02 = 512; 00 03 = 768)
            byte[] body = { 0x00, 0x01, 0x00, 0x02, 0x00, 0x03, 0x00, 0x00 }; 
            ushort sampleSize = 16; 

            // Act
            var samples = NetSdrMessageHelper.GetSamples(sampleSize, body).ToList();

            // Assert
            Assert.That(samples.Count, Is.EqualTo(4));
            Assert.That(samples[0], Is.EqualTo(256));
            Assert.That(samples[1], Is.EqualTo(512));
            Assert.That(samples[2], Is.EqualTo(768));
            Assert.That(samples[3], Is.EqualTo(0)); 
        }

        [Test]
        public void GetSamples_HandlesIncompleteSampleAtEnd()
        {
            // Arrange (Масив: 00 01 00 02 00) -> 2 повних семпли (0x0100 і 0x0200)
            byte[] body = { 0x00, 0x01, 0x00, 0x02, 0x00 }; 
            ushort sampleSize = 16; // 2 байти

            // Act
            var samples = NetSdrMessageHelper.GetSamples(sampleSize, body).ToList();

            // Assert
            Assert.That(samples.Count, Is.EqualTo(2)); 
            Assert.That(samples[0], Is.EqualTo(256)); // 0x00, 0x01
            Assert.That(samples[1], Is.EqualTo(512)); // 0x00, 0x02
        }

        //TODO: add more NetSdrMessageHelper tests
    }
}