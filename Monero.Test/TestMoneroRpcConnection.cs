using Monero.Common;
using Monero.Test.Utils;

namespace Monero.Test
{
    public class TestMoneroRpcConnection
    {
        // Can copy connection
        [Fact]
        public void TestClone()
        {
            var connection = new MoneroRpcConnection("test", "user", "pass123", "test_zmq", 2);

            var copy = connection.Clone();
            
            Assert.True(connection.Equals(copy));

            connection = new MoneroRpcConnection(TestUtils.DAEMON_RPC_URI);
            
            TestConnection(connection, TestUtils.DAEMON_RPC_URI, true);

            copy = connection.Clone();

            Assert.True(connection.Equals(copy));
        }

        // Can connect to node
        [Fact]
        public void TestNodeRpcConnection()
        {
            var connection = new MoneroRpcConnection(TestUtils.DAEMON_RPC_URI);
            
            TestConnection(connection, TestUtils.DAEMON_RPC_URI, true);
        }
        
        // Can connect to wallet
        [Fact]
        public void TestWalletRpcConnection()
        {
            var connection = new MoneroRpcConnection(TestUtils.WALLET_RPC_URI);
            
            TestConnection(connection, TestUtils.WALLET_RPC_URI, true);
        }
        
        // Test invalid url
        [Fact]
        public void TestInvalidConnection()
        {
            var connection = new MoneroRpcConnection("");

            TestConnection(connection, "", false);
        }
        
        // Can send request to RPC
        [Fact]
        public void TestSendRequest()
        {
            // Setup connection

            var connection = new MoneroRpcConnection(TestUtils.DAEMON_RPC_URI);

            TestConnection(connection,  TestUtils.DAEMON_RPC_URI, true);

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

        private static void TestConnection(MoneroRpcConnection? connection, string? uri, bool online)
        {
            Assert.NotNull(connection);
            
            if (online)
            {
                Assert.NotNull(uri);
                Assert.NotEmpty(uri);
                Assert.Equal(uri, connection.GetUri());
                Assert.True(connection.IsClearnet());
                Assert.False(connection.IsOnion());
                Assert.False(connection.IsI2P());
                Assert.True(connection.CheckConnection());
                Assert.True(connection.IsOnline());
                Assert.True(connection.IsAuthenticated());
                Assert.True(connection.IsConnected());
            }
            else
            {
                Assert.Equal(uri, connection.GetUri());
                Assert.False(connection.IsClearnet());
                Assert.False(connection.IsOnion());
                Assert.False(connection.IsI2P());
                Assert.True(connection.CheckConnection());
                Assert.False(connection.IsOnline());
                Assert.Null(connection.IsAuthenticated());
                Assert.False(connection.IsConnected());
            }
        }
        
    }
}
