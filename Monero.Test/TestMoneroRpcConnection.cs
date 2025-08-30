using Monero.Common;

namespace Monero.Test
{
    public class TestMoneroRpcConnection
    {
        [Fact]
        public void TestClone()
        {
            var connection = new MoneroRpcConnection("test", "user", "pass123", "test_zmq", 2);

            var copy = connection.Clone();
            
            Assert.True(connection.Equals(copy));

            connection = new MoneroRpcConnection("http://xmr.gn.gy:18089");
            Assert.Equal("http://xmr.gn.gy:18089", connection.GetUri());
            Assert.True(connection.IsClearnet());
            Assert.False(connection.IsOnion());
            Assert.False(connection.IsI2p());
            Assert.True(connection.CheckConnection());
            Assert.True(connection.IsOnline());
            Assert.True(connection.IsAuthenticated());
            Assert.True(connection.IsConnected());

            copy = connection.Clone();

            Assert.True(connection.Equals(copy));
        }

        [Fact]
        public void TestCheckConnection()
        {
            // Test HTTP connection

            var connection = new MoneroRpcConnection("http://xmr.gn.gy:18089");

            Assert.Equal("http://xmr.gn.gy:18089", connection.GetUri());
            Assert.True(connection.IsClearnet());
            Assert.False(connection.IsOnion());
            Assert.False(connection.IsI2p());
            Assert.True(connection.CheckConnection());
            Assert.True(connection.IsOnline());
            Assert.True(connection.IsAuthenticated());
            Assert.True(connection.IsConnected());

            connection = new MoneroRpcConnection("http://xmr-de.boldsuck.org:18081");

            Assert.Equal("http://xmr-de.boldsuck.org:18081", connection.GetUri());
            Assert.True(connection.IsClearnet());
            Assert.False(connection.IsOnion());
            Assert.False(connection.IsI2p());
            Assert.True(connection.CheckConnection());
            Assert.True(connection.IsOnline());
            Assert.True(connection.IsAuthenticated());
            Assert.True(connection.IsConnected());

            // Test HTTPS connection

            connection = new MoneroRpcConnection("https://xmr-node.cakewallet.com:18081");

            Assert.Equal("https://xmr-node.cakewallet.com:18081", connection.GetUri());
            Assert.True(connection.IsClearnet());
            Assert.False(connection.IsOnion());
            Assert.False(connection.IsI2p());
            Assert.True(connection.CheckConnection());
            Assert.True(connection.IsOnline());
            Assert.True(connection.IsAuthenticated());
            Assert.True(connection.IsConnected());

            connection = new MoneroRpcConnection("https://node.sethforprivacy.com");

            Assert.Equal("https://node.sethforprivacy.com", connection.GetUri());
            Assert.True(connection.IsClearnet());
            Assert.False(connection.IsOnion());
            Assert.False(connection.IsI2p());
            Assert.True(connection.CheckConnection());
            Assert.True(connection.IsOnline());
            Assert.True(connection.IsAuthenticated());
            Assert.True(connection.IsConnected());

            // Test invalid url

            connection.SetUri("");

            Assert.Equal("", connection.GetUri());
            Assert.False(connection.IsClearnet());
            Assert.False(connection.IsOnion());
            Assert.False(connection.IsI2p());
            Assert.True(connection.CheckConnection());
            Assert.False(connection.IsOnline());
            Assert.Null(connection.IsAuthenticated());
            Assert.False(connection.IsConnected());
        }

        [Fact]
        public void TestSendRequest()
        {
            // Setup connection

            var connection = new MoneroRpcConnection("http://xmr.gn.gy:18089");

            Assert.Equal("http://xmr.gn.gy:18089", connection.GetUri());
            Assert.True(connection.IsClearnet());
            Assert.False(connection.IsOnion());
            Assert.False(connection.IsI2p());
            Assert.True(connection.CheckConnection());
            Assert.True(connection.IsOnline());
            Assert.True(connection.IsAuthenticated());
            Assert.True(connection.IsConnected());

            // Test monerod JSON request

            var jsonResponse = connection.SendJsonRequest("get_info");

            Assert.NotNull(jsonResponse);
            Assert.Null(jsonResponse.Error);
            Assert.NotNull(jsonResponse.Result);

            // Test monerod PATH request

            var pathResponse = connection.SendPathRequest("get_info");

            Assert.NotNull(pathResponse);
            Assert.Null(pathResponse.GetValueOrDefault("error"));

            // Test monerod BINARY request

            // TODO implement MoneroRpcConnection.SendBinaryRequest()

            //var binaryResponse = connection.SendBinaryRequest("get_outs.bin");
            //Assert.NotNull(binaryResponse);
        }

    }
}
