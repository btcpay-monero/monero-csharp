using Monero.Common;
using Monero.IntegrationTests.Utils;
using Monero.Wallet;

namespace Monero.IntegrationTests;

public class TestMoneroConnectionManager
{
    [Fact]
    public async Task TestConnectionManager()
    {
        List<MoneroWalletRpc> walletRpcs = [];
        MoneroConnectionManager? connectionManager = null;

        try
        {
            // start monero-wallet-rpc instances as test server connections (can also use monerod servers)
            for (int i = 0; i < 5; i++)
            {
                walletRpcs.Add(TestUtils.StartWalletRpcProcess());
            }

            // create connection manager
            connectionManager = new MoneroConnectionManager();

            // listen for changes
            ConnectionChangeCollector listener = new();
            connectionManager.AddListener(listener);

            // add prioritized connections
            connectionManager.AddConnection(walletRpcs[4].GetRpcConnection().SetPriority(1));
            connectionManager.AddConnection(walletRpcs[2].GetRpcConnection().SetPriority(2));
            connectionManager.AddConnection(walletRpcs[3].GetRpcConnection().SetPriority(2));
            connectionManager.AddConnection(walletRpcs[0].GetRpcConnection()); // default priority is lowest
            connectionManager.AddConnection(
                new MoneroRpcConnection(walletRpcs[1].GetRpcConnection().GetUri())); // test unauthenticated

            // test connections and order
            List<MoneroRpcConnection> orderedConnections = connectionManager.GetRpcConnections();
            Assert.True(orderedConnections[0] == walletRpcs[4].GetRpcConnection());
            Assert.True(orderedConnections[1] == walletRpcs[2].GetRpcConnection());
            Assert.True(orderedConnections[2] == walletRpcs[3].GetRpcConnection());
            Assert.True(orderedConnections[3] == walletRpcs[0].GetRpcConnection());
            Assert.True(orderedConnections[4].GetUri() == walletRpcs[1].GetRpcConnection().GetUri());
            foreach (MoneroRpcConnection c in orderedConnections)
            {
                Assert.NotNull(c.IsOnline());
            }

            // test getting connection by uri
            Assert.True(connectionManager.HasConnection(walletRpcs[0].GetRpcConnection().GetUri()));
            Assert.True(connectionManager.GetConnection(walletRpcs[0].GetRpcConnection().GetUri()) ==
                        walletRpcs[0].GetRpcConnection());

            // test unknown connection
            int numExpectedChanges = 0;
            connectionManager.SetConnection(orderedConnections[0]);
            Assert.Null(connectionManager.IsConnected());
            numExpectedChanges++;
            Assert.True(numExpectedChanges == listener.ChangedConnections.Count);

            // auto connect to the best available connection
            connectionManager.StartPolling((ulong)TestUtils.SYNC_PERIOD_IN_MS);
            Thread.Sleep(TestUtils.AUTO_CONNECT_TIMEOUT_MS);
            Assert.True(connectionManager.IsConnected());
            MoneroRpcConnection? connection = connectionManager.GetConnection();
            Assert.NotNull(connection);
            Assert.True(connection.IsOnline() == true);
            Assert.True(connection == walletRpcs[4].GetRpcConnection());
            numExpectedChanges++;
            Assert.True(numExpectedChanges == listener.ChangedConnections.Count);
            Assert.True(listener.ChangedConnections[listener.ChangedConnections.Count - 1] == connection);
            connectionManager.SetAutoSwitch(false);
            connectionManager.StopPolling();
            connectionManager.Disconnect();
            numExpectedChanges++;
            Assert.True(numExpectedChanges == listener.ChangedConnections.Count);
            Assert.True(listener.ChangedConnections[listener.ChangedConnections.Count - 1] == null);

            // start periodically checking connection without auto switch
            connectionManager.StartPolling((ulong)TestUtils.SYNC_PERIOD_IN_MS, false);

            // connect to the best available connection in order of priority and response time
            connection = connectionManager.GetBestAvailableConnection();
            connectionManager.SetConnection(connection);
            Assert.True(connection == walletRpcs[4].GetRpcConnection());
            numExpectedChanges = TestExpectedChanges(connection, listener, numExpectedChanges);

            // test connections and order
            orderedConnections = connectionManager.GetRpcConnections();
            Assert.True(orderedConnections[0] == walletRpcs[4].GetRpcConnection());
            Assert.True(orderedConnections[1] == walletRpcs[2].GetRpcConnection());
            Assert.True(orderedConnections[2] == walletRpcs[3].GetRpcConnection());
            Assert.True(orderedConnections[3] == walletRpcs[0].GetRpcConnection());
            Assert.True(orderedConnections[4].GetUri() == walletRpcs[1].GetRpcConnection().GetUri());
            for (int i = 1; i < orderedConnections.Count; i++)
            {
                Assert.Null(orderedConnections[i].IsOnline());
            }

            // shut down prioritized servers
            TestUtils.StopWalletRpcProcess(walletRpcs[2]);
            TestUtils.StopWalletRpcProcess(walletRpcs[3]);
            TestUtils.StopWalletRpcProcess(walletRpcs[4]);
            Thread.Sleep(TestUtils.SYNC_PERIOD_IN_MS + 100); // allow time to poll
            Assert.False(connectionManager.IsConnected());
            Assert.False(connectionManager.GetConnection()!.IsOnline());
            Assert.Null(connectionManager.GetConnection()!.IsAuthenticated());
            numExpectedChanges++;
            Assert.True(numExpectedChanges == listener.ChangedConnections.Count);
            Assert.True(listener.ChangedConnections[listener.ChangedConnections.Count - 1] ==
                        connectionManager.GetConnection());

            // test connection order
            orderedConnections = connectionManager.GetRpcConnections();
            TestConnectionOrder(walletRpcs, orderedConnections, [4, 0, 1, 2, 3], 2);

            // check all connections
            await connectionManager.CheckConnections();

            // test connection order
            orderedConnections = connectionManager.GetRpcConnections();
            TestConnectionOrder(walletRpcs, orderedConnections, [4, 0, 1, 2, 3], 2);

            // test online and authentication status
            for (int i = 0; i < orderedConnections.Count; i++)
            {
                bool? IsOnline = orderedConnections[i].IsOnline();
                bool? IsAuthenticated = orderedConnections[i].IsAuthenticated();
                if (i == 1 || i == 2)
                {
                    Assert.True(IsOnline);
                }
                else
                {
                    Assert.False(IsOnline);
                }

                if (i == 1)
                {
                    Assert.True(IsAuthenticated);
                }
                else if (i == 2)
                {
                    Assert.False(IsAuthenticated);
                }
                else
                {
                    Assert.Null(IsAuthenticated);
                }
            }

            // test auto switch when disconnected
            connectionManager.SetAutoSwitch(true);
            Thread.Sleep(TestUtils.SYNC_PERIOD_IN_MS + 100);
            Assert.True(connectionManager.IsConnected());
            connection = connectionManager.GetConnection();
            Assert.NotNull(connection);
            Assert.True(connection.IsOnline());
            Assert.True(connection == walletRpcs[0].GetRpcConnection());
            numExpectedChanges++;
            Assert.True(numExpectedChanges == listener.ChangedConnections.Count);
            Assert.True(listener.ChangedConnections[listener.ChangedConnections.Count - 1] == connection);

            // test connection order
            orderedConnections = connectionManager.GetRpcConnections();
            TestConnectionOrder(walletRpcs, connection, orderedConnections, [0, 1, 4, 2, 3], 1, true);

            // connect to a specific endpoint without authentication
            connection = orderedConnections[1];
            Assert.False(connection.IsAuthenticated());
            connectionManager.SetConnection(connection);
            Assert.False(connectionManager.IsConnected());
            numExpectedChanges++;
            Assert.True(numExpectedChanges == listener.ChangedConnections.Count);

            // connect to a specific endpoint with authentication
            orderedConnections[1].SetCredentials("rpc_user", "abc123");
            await connectionManager.CheckConnections();
            Assert.True(connectionManager.GetConnection()!.GetUri() == walletRpcs[1].GetRpcConnection().GetUri());
            numExpectedChanges = TestExpectedChanges(connection, listener, numExpectedChanges);

            // test connection order
            orderedConnections = connectionManager.GetRpcConnections();
            TestConnectionOrder(walletRpcs, connectionManager.GetConnection(), orderedConnections, [1, 0, 4, 2, 3], 0, true);

            for (int i = 0; i < orderedConnections.Count - 1; i++)
            {
                Assert.True(
                    i <= 1 ? orderedConnections[i].IsOnline() == true : orderedConnections[i].IsOnline() != true);
            }

            Assert.False(orderedConnections[4].IsOnline());

            // set connection to existing uri
            connectionManager.SetConnection(walletRpcs[0].GetRpcConnection()!.GetUri()!);
            Assert.True(connectionManager.IsConnected());
            Assert.True(walletRpcs[0].GetRpcConnection() == connectionManager.GetConnection());

            MoneroRpcConnection? currentConnection = connectionManager.GetConnection();
            MoneroRpcConnection rpcConnection = new();

            if (rpcConnection.GetType().IsInstanceOfType(currentConnection))
            {
                rpcConnection = currentConnection;
                Assert.True(TestUtils.WALLET_RPC_USERNAME == rpcConnection.GetUsername());
                Assert.True(TestUtils.WALLET_RPC_PASSWORD == rpcConnection.GetPassword());
            }

            numExpectedChanges++;
            Assert.True(numExpectedChanges == listener.ChangedConnections.Count);
            Assert.True(listener.ChangedConnections[listener.ChangedConnections.Count - 1] ==
                        walletRpcs[0].GetRpcConnection());

            // set connection to new uri
            connectionManager.StopPolling();
            string uri = "http://localhost:49999";
            connectionManager.SetConnection(uri);
            Assert.True(uri == connectionManager.GetConnection()!.GetUri());
            numExpectedChanges++;
            Assert.True(numExpectedChanges == listener.ChangedConnections.Count);
            Assert.True(uri == listener.ChangedConnections[listener.ChangedConnections.Count - 1]!.GetUri());

            // set connection to an empty string
            connectionManager.SetConnection("");
            Assert.Null(connectionManager.GetConnection());
            numExpectedChanges++;
            Assert.True(numExpectedChanges == listener.ChangedConnections.Count);

            // check all connections and test auto switch
            connectionManager.CheckConnections();
            numExpectedChanges++;
            Assert.True(numExpectedChanges == listener.ChangedConnections.Count);
            Assert.True(connectionManager.IsConnected());

            // remove the current connection and test auto switch
            connectionManager.RemoveConnection(connectionManager.GetConnection()!.GetUri());
            numExpectedChanges++;
            Assert.True(numExpectedChanges == listener.ChangedConnections.Count);
            Assert.False(connectionManager.IsConnected());
            connectionManager.CheckConnections();
            numExpectedChanges++;
            Assert.True(numExpectedChanges == listener.ChangedConnections.Count);
            Assert.True(connectionManager.IsConnected());

            // test polling current connection
            MoneroRpcConnection? nullConnection = null;
            connectionManager.SetConnection(nullConnection);
            Assert.False(connectionManager.IsConnected());
            numExpectedChanges++;
            Assert.True(numExpectedChanges == listener.ChangedConnections.Count);
            connectionManager.StartPolling((ulong)TestUtils.SYNC_PERIOD_IN_MS, null, null,
                MoneroConnectionManager.PollType.Current);
            Thread.Sleep(TestUtils.AUTO_CONNECT_TIMEOUT_MS);
            Assert.True(connectionManager.IsConnected());
            numExpectedChanges++;
            Assert.True(numExpectedChanges == listener.ChangedConnections.Count);

            // test polling all connections
            connectionManager.SetConnection(nullConnection);
            numExpectedChanges++;
            Assert.True(numExpectedChanges == listener.ChangedConnections.Count);
            connectionManager.StartPolling((ulong)TestUtils.SYNC_PERIOD_IN_MS, null, null,
                MoneroConnectionManager.PollType.All);
            Thread.Sleep(TestUtils.AUTO_CONNECT_TIMEOUT_MS);
            Assert.True(connectionManager.IsConnected());
            numExpectedChanges++;
            Assert.True(numExpectedChanges == listener.ChangedConnections.Count);

            // shut down all connections
            connection = connectionManager.GetConnection();
            Assert.NotNull(connection);
            foreach (MoneroWalletRpc walletRpc in walletRpcs)
            {
                TestUtils.StopWalletRpcProcess(walletRpc);
            }

            Thread.Sleep(TestUtils.SYNC_PERIOD_IN_MS + 100);
            Assert.False(connection.IsOnline());
            numExpectedChanges++;
            Assert.True(numExpectedChanges == listener.ChangedConnections.Count);
            Assert.True(listener.ChangedConnections[listener.ChangedConnections.Count - 1] == connection);

            // reset
            connectionManager.Reset();
            Assert.True(0 == connectionManager.GetConnections().Count);
            Assert.Null(connectionManager.GetConnection());
        }
        catch (Exception ex)
        {
            Assert.Fail($"An exception occurred: {ex.Message}");
        }
        finally
        {
            if (connectionManager != null)
            {
                connectionManager.Reset();
            }

            foreach (MoneroWalletRpc wallet in walletRpcs)
            {
                try
                {
                    TestUtils.StopWalletRpcProcess(wallet);
                }
                catch
                {
                    // ignore
                }
            }
        }
    }

    private static int TestExpectedChanges(MoneroRpcConnection? connection, ConnectionChangeCollector listener, int numExpectedChanges)
    {
        if (connection == null)
        {
            throw new MoneroError("Connection is null");
        }

        Assert.True(connection.IsOnline());
        Assert.True(connection.IsAuthenticated());
        int expectedChanges = numExpectedChanges;
        Assert.True(numExpectedChanges == listener.ChangedConnections.Count);
        Assert.True(listener.ChangedConnections.Last() == connection);

        return expectedChanges;
    }

    private static void TestConnectionOrder(List<MoneroWalletRpc> walletRpcs,
        List<MoneroRpcConnection> orderedConnections, List<int> indices, int checkUriIndex)
    {
        TestConnectionOrder(walletRpcs, null, orderedConnections, indices, checkUriIndex, false);
    }

    private static void TestConnectionOrder(List<MoneroWalletRpc> walletRpcs, MoneroRpcConnection? connection, List<MoneroRpcConnection> orderedConnections, List<int> indices, int checkUriIndex, bool checkConnection)
    {
        if (indices.Count != orderedConnections.Count || indices.Count != walletRpcs.Count)
        {
            throw new MoneroError("Invalid indices");
        }

        if (checkConnection)
        {
            Assert.True(orderedConnections[0] == connection);
        }

        int i = 0;

        foreach (MoneroRpcConnection orderedConnection in orderedConnections)
        {
            int walletIndex = indices[i];

            if (checkUriIndex == i)
            {
                Assert.True(orderedConnection.GetUri() == walletRpcs[walletIndex].GetRpcConnection().GetUri());
            }
            else
            {
                Assert.True(orderedConnection == walletRpcs[walletIndex].GetRpcConnection());
            }

            i++;
        }
    }

}

internal class ConnectionChangeCollector : MoneroConnectionManagerListener
{
    public List<MoneroRpcConnection?> ChangedConnections = [];

    public void OnConnectionChanged(MoneroRpcConnection? connection)
    {
        ChangedConnections.Add(connection);
    }
}