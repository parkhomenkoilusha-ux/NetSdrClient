using Moq;
using NetSdrClientApp;
using NetSdrClientApp.Networking;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace NetSdrClientAppTests
{
    [TestFixture]
    public class NetSdrClientTests
    {
        private Mock<ITcpClient> _mockTcp;
        private Mock<IUdpClient> _mockUdp;
        private NetSdrClient _client;

        [SetUp]
        public void Setup()
        {
            _mockTcp = new Mock<ITcpClient>();
            _mockUdp = new Mock<IUdpClient>();

            _mockTcp.Setup(x => x.SendMessageAsync(It.IsAny<byte[]>()))
                .Returns(Task.CompletedTask)
                .Callback(() => 
                {
                    _mockTcp.Raise(m => m.MessageReceived += null, _mockTcp.Object, new byte[] { 0x00 });
                });

            _client = new NetSdrClient(_mockTcp.Object, _mockUdp.Object);
        }

        [Test]
        public async Task ConnectAsync_ShouldCallConnect_WhenNotConnected()
        {
            _mockTcp.Setup(x => x.Connected).Returns(false);
            await _client.ConnectAsync();
            _mockTcp.Verify(x => x.Connect(), Times.Once);
        }

        [Test]
        public async Task ConnectAsync_ShouldNotCallConnect_WhenAlreadyConnected()
        {
            _mockTcp.Setup(x => x.Connected).Returns(true);
            await _client.ConnectAsync();
            _mockTcp.Verify(x => x.Connect(), Times.Never);
        }

        [Test]
        public async Task StartIQAsync_ShouldStartUdp_WhenConnected()
        {
            _mockTcp.Setup(x => x.Connected).Returns(true);
            await _client.StartIQAsync();
            _mockUdp.Verify(x => x.StartListeningAsync(), Times.Once);
            Assert.IsTrue(_client.IQStarted);
        }

        [Test]
        public async Task StartIQAsync_ShouldDoNothing_WhenNotConnected()
        {
            _mockTcp.Setup(x => x.Connected).Returns(false);
            await _client.StartIQAsync();
            _mockUdp.Verify(x => x.StartListeningAsync(), Times.Never);
        }

        [Test]
        public async Task StopIQAsync_ShouldStopUdp_WhenConnected()
        {
            _mockTcp.Setup(x => x.Connected).Returns(true);
            await _client.StopIQAsync();
            _mockUdp.Verify(x => x.StopListening(), Times.Once);
            Assert.IsFalse(_client.IQStarted);
        }

        [Test]
        public async Task ChangeFrequencyAsync_ShouldSendRequest_WhenConnected()
        {
            _mockTcp.Setup(x => x.Connected).Returns(true);
            await _client.ChangeFrequencyAsync(100000, 0);
            _mockTcp.Verify(x => x.SendMessageAsync(It.IsAny<byte[]>()), Times.AtLeastOnce);
        }
        
        [Test]
        public void Disconnect_ShouldCallTcpDisconnect()
        {
            _client.Disconect();
            _mockTcp.Verify(x => x.Disconnect(), Times.Once);
        }
    }
}