using System.Collections.ObjectModel;

namespace Monero.Common;

public class MoneroConnectionManager
{
    private static readonly ulong DEFAULT_TIMEOUT = 5000;
    private static readonly ulong DEFAULT_POLL_PERIOD = 20000;
    private static readonly bool DEFAULT_AUTO_SWITCH = true;
    private static readonly int MIN_BETTER_RESPONSES = 3;

    public enum PollType
    {
        PRIORITIZED,
        CURRENT,
        ALL
    }

    private class ConnectionPriorityComparator : IComparer<int>
    {
        public int Compare(int p1, int p2)
        {
            if (p1 == p2) return 0;
            if (p1 == 0) return -1;
            if (p2 == 0) return 1;
            return p2 - p1;
        }
    }

    private class ConnectionComparator : Comparer<MoneroRpcConnection>
    {
        public MoneroRpcConnection? currentConnection;
        public ConnectionPriorityComparator priorityComparator = new ConnectionPriorityComparator();

        public override int Compare(MoneroRpcConnection? c1, MoneroRpcConnection? c2)
        {

            // current connection is first
            if (c1 == currentConnection) return -1;
            if (c2 == currentConnection) return 1;
            if (c1 == null && c2 == null) return 0;
            if (c1 == null) return 1;
            if (c2 == null) return -1;

            // order by availability then priority then by name
            if (c1.IsOnline() == c2.IsOnline())
            {
                if (c1.GetPriority() == c2.GetPriority()) return c1.GetUri().CompareTo(c2.GetUri());
                return priorityComparator.Compare(c1.GetPriority(), c2.GetPriority()) * -1; // order by priority in descending order
            }
            else
            {
                if (c1.IsOnline() == true) return -1;
                else if (c2.IsOnline() == true) return 1;
                else if (c1.IsOnline() == null) return -1;
                else return 1; // c1 is offline
            }
        }

    }

    private TaskLooper? poller;
    private readonly object _listenersLock = new();
    private readonly object _connectionsLock = new();

    private MoneroRpcConnection? _currentConnection;
    private readonly List<MoneroRpcConnection> _connections = [];
    private readonly List<MoneroConnectionManagerListener> _listeners = [];
    //private ConnectionComparator _connectionComparator = new ConnectionComparator();
    private bool _autoSwitch = DEFAULT_AUTO_SWITCH;
    private ulong _timeoutMs = DEFAULT_TIMEOUT;
    //private TaskLooper poller;
    private readonly Dictionary<MoneroRpcConnection, List<ulong?>> _responseTimes = [];

    private void OnConnectionChanged(MoneroRpcConnection? connection)
    {
        lock (_listenersLock)
        {
            foreach (var listener in _listeners)
            {
                listener.OnConnectionChanged(connection);
            }
        }
    }

    private MoneroRpcConnection? GetBestConnectionFromPrioritizedResponses(List<MoneroRpcConnection> responses)
    {

        // get best response
        MoneroRpcConnection? bestResponse = null;

        foreach (MoneroRpcConnection connection in responses)
        {
            if (connection.IsConnected() == true && (bestResponse == null || connection.GetResponseTime() < bestResponse.GetResponseTime())) bestResponse = connection;
        }

        // no update if no responses
        if (bestResponse == null) return null;

        // use best response if disconnected
        MoneroRpcConnection bestConnection = GetConnection();
        if (bestConnection == null || bestConnection.IsConnected() != true) return bestResponse;

        var priorityComparator = new ConnectionPriorityComparator();

        // use best response if different priority (assumes being called in descending priority)
        if (priorityComparator.Compare(bestResponse.GetPriority(), bestConnection.GetPriority()) != 0) return bestResponse;

        // keep best connection if not enough data
        if (!_responseTimes.ContainsKey(bestConnection)) return bestConnection;

        // check if a connection is consistently better
        foreach (MoneroRpcConnection connection in responses)
        {
            if (connection == bestConnection) continue;
            if (!_responseTimes.ContainsKey(connection) || _responseTimes[connection].Count < MIN_BETTER_RESPONSES) continue;
            bool better = true;
            for (int i = 0; i < MIN_BETTER_RESPONSES; i++)
            {
                if (_responseTimes[connection][i] == null || _responseTimes[bestConnection][i] == null || _responseTimes[connection][i] > _responseTimes[bestConnection][i])
                {
                    better = false;
                    break;
                }
            }
            if (better) bestConnection = connection;
        }
        return bestConnection;
    }

    private MoneroRpcConnection? UpdateBestConnectionInPriority()
    {
        if (!_autoSwitch) return null;
        foreach (List<MoneroRpcConnection> prioritizedConnections in GetConnectionsInAscendingPriority())
        {
            MoneroRpcConnection bestConnectionFromResponses = GetBestConnectionFromPrioritizedResponses(prioritizedConnections);
            if (bestConnectionFromResponses != null)
            {
                SetConnection(bestConnectionFromResponses);
                return bestConnectionFromResponses;
            }
        }

        return null;
    }

    private MoneroRpcConnection? ProcessResponses(Collection<MoneroRpcConnection> responses)
    {

        // add new connections
        foreach (MoneroRpcConnection connection in responses)
        {
            if (!_responseTimes.ContainsKey(connection)) _responseTimes.Add(connection, []);
        }

        // insert response times or null
        foreach (KeyValuePair<MoneroRpcConnection, List<ulong?>> responseTime in _responseTimes.ToList())
        {
            responseTime.Value.Add(0);
            responseTime.Value.Add(responses.Contains(responseTime.Key) ? responseTime.Key.GetResponseTime() : null);

            // remove old response times
            if (responseTime.Value.Count > MIN_BETTER_RESPONSES) responseTime.Value.RemoveAt(responseTime.Value.Count - 1);
        }

        // update best connection based on responses and priority
        return UpdateBestConnectionInPriority();
    }

    private List<List<MoneroRpcConnection>> GetConnectionsInAscendingPriority()
    {
        lock (_connectionsLock)
        {
            Dictionary<int, List<MoneroRpcConnection>> connectionPriorities = [];

            foreach (MoneroRpcConnection connection in _connections)
            {
                if (!connectionPriorities.ContainsKey(connection.GetPriority())) connectionPriorities.Add(connection.GetPriority(), new List<MoneroRpcConnection>());
                connectionPriorities[connection.GetPriority()].Add(connection);
            }
            List<List<MoneroRpcConnection>> prioritizedConnections = [];
            foreach (List<MoneroRpcConnection> priorityConnections in connectionPriorities.Values) prioritizedConnections.Add(priorityConnections);
            if (connectionPriorities.ContainsKey(0))
            {
                var first = prioritizedConnections[0]; // move priority 0 to end
                prioritizedConnections.RemoveAt(0);
                prioritizedConnections.Add(first); // move priority 0 to end
            }
            return prioritizedConnections;
        }
    }

    private bool CheckConnections(List<MoneroRpcConnection> connections, List<MoneroRpcConnection>? excludedConnections)
    {
        throw new NotImplementedException("");
    }

    private void CheckPrioritizedConnections(List<MoneroRpcConnection>? excludedConnections)
    {
        foreach (List<MoneroRpcConnection> prioritizedConnections in GetConnectionsInAscendingPriority())
        {
            bool hasConnection = CheckConnections(prioritizedConnections, excludedConnections);
            if (hasConnection) return;
        }
    }

    private void StartPollingConnection(ulong periodMs)
    {
        poller = new TaskLooper(() =>
        {
            try { CheckConnection(); }
            catch (Exception e) { MoneroUtils.Log(0, e.StackTrace != null ? e.StackTrace : ""); }
        });
        poller.Start(periodMs);
    }

    private void StartPollingConnections(ulong periodMs)
    {
        poller = new TaskLooper(() =>
        {
            try { CheckConnections(); }
            catch (Exception e) { MoneroUtils.Log(0, e.StackTrace != null ? e.StackTrace : ""); }
        });
        poller.Start(periodMs);
    }

    private void StartPollingPrioritizedConnections(ulong periodMs, List<MoneroRpcConnection>? excludedConnections = null)
    {
        poller = new TaskLooper(() =>
        {
            try { CheckPrioritizedConnections(excludedConnections); }
            catch (Exception e) { MoneroUtils.Log(0, e.StackTrace != null ? e.StackTrace : ""); }
        });
        poller.Start(periodMs);
    }

    public MoneroConnectionManager StartPolling(ulong? periodMs = null, bool? autoSwitch = null, ulong? timeoutMs = null, PollType? pollType = null, List<MoneroRpcConnection>? excludedConnections = null)
    {

        // apply defaults
        if (periodMs == null) periodMs = DEFAULT_POLL_PERIOD;
        if (autoSwitch != null) SetAutoSwitch((bool)autoSwitch);
        if (timeoutMs != null) SetTimeout(timeoutMs);
        if (pollType == null) pollType = PollType.PRIORITIZED;

        // stop polling
        StopPolling();

        // start polling
        switch (pollType)
        {
            case PollType.CURRENT:
                StartPollingConnection((ulong)periodMs);
                break;
            case PollType.ALL:
                StartPollingConnections((ulong)periodMs);
                break;
            case PollType.PRIORITIZED:
            default:
                StartPollingPrioritizedConnections((ulong)periodMs, excludedConnections);
                break;
        }
        return this;
    }

    public MoneroConnectionManager StopPolling()
    {
        if (poller != null) poller.Stop();
        poller = null;
        return this;
    }

    public ulong GetTimeout()
    {
        return _timeoutMs;
    }

    public MoneroConnectionManager SetTimeout(ulong? timeoutMs)
    {
        if (timeoutMs == null)
        {
            _timeoutMs = DEFAULT_TIMEOUT;
            return this;
        }
        _timeoutMs = (ulong)timeoutMs;
        return this;
    }

    public bool GetAutoSwitch()
    {
        return _autoSwitch;
    }

    public MoneroConnectionManager SetAutoSwitch(bool autoSwitch)
    {
        _autoSwitch = autoSwitch;
        return this;
    }

    public bool HasListeners()
    {
        lock (_listenersLock)
        {
            return _listeners.Count > 0;
        }
    }

    public bool HasListener(MoneroConnectionManagerListener listener)
    {
        lock (_listenersLock)
        {
            return _listeners.Contains(listener);
        }
    }

    public MoneroConnectionManager AddListener(MoneroConnectionManagerListener listener)
    {
        lock (_listenersLock)
        {
            if (!_listeners.Contains(listener))
            {
                _listeners.Add(listener);
            }
        }
        return this;
    }

    public MoneroConnectionManager RemoveListener(MoneroConnectionManagerListener listener)
    {
        lock (_listenersLock)
        {
            _listeners.Remove(listener);
        }
        return this;
    }

    public MoneroConnectionManager RemoveListeners()
    {
        lock (_listenersLock)
        {
            _listeners.Clear();
        }
        return this;
    }

    public List<MoneroConnectionManagerListener> GetListeners()
    {
        lock (_listenersLock)
        {
            return [.. _listeners];
        }
    }

    public bool HasConnections()
    {
        lock (_connectionsLock)
        {
            return _connections.Count > 0;
        }
    }

    public bool HasConnection(MoneroRpcConnection connection)
    {
        lock (_connectionsLock)
        {
            return _connections.Contains(connection);
        }
    }

    public bool HasConnection(string? uri)
    {
        lock (_connectionsLock)
        {
            return _connections.Any(c => c.GetUri() == uri);
        }
    }

    public bool? IsConnected()
    {
        lock (_connectionsLock)
        {
            if (_currentConnection == null) return false;

            return _currentConnection.IsConnected();
        }
    }

    public MoneroConnectionManager AddConnection(MoneroRpcConnection connection)
    {
        lock (_connectionsLock)
        {
            if (!_connections.Contains(connection))
            {
                _connections.Add(connection);
            }
        }
        return this;
    }

    public MoneroConnectionManager AddRpcConnection(string uri, string? username = null, string? password = null, string? zmqUri = null)
    {
        return AddConnection(new MoneroRpcConnection(uri, username, password, zmqUri));
    }

    public MoneroRpcConnection? GetConnection(string? uri)
    {
        lock (_connectionsLock)
        {
            return _connections.FirstOrDefault(c => c.GetUri() == uri);
        }
    }

    public MoneroConnectionManager SetConnection(MoneroRpcConnection? connection)
    {
        lock (_connectionsLock)
        {
            if (_currentConnection == connection) return this;

            if (connection == null)
            {
                _currentConnection = null;
                OnConnectionChanged(null);
                return this;
            }

            string? uri = connection.GetUri();

            if (string.IsNullOrEmpty(uri)) throw new MoneroError("Connection is missing URI");

            MoneroRpcConnection? existingConnection = GetConnection(uri);
            if (existingConnection != null)
            {
                RemoveConnection(existingConnection);
            }

            AddConnection(connection);
            _currentConnection = connection;
            OnConnectionChanged(connection);
        }

        return this;
    }

    public MoneroConnectionManager SetConnection(string uri, string? username = null, string? password = null, string? zmqUri = null)
    {
        return SetConnection(new MoneroRpcConnection(uri, username, password, zmqUri));
    }

    public MoneroRpcConnection? GetConnection()
    {
        lock (_connectionsLock)
        {
            return _currentConnection;
        }
    }

    public List<MoneroRpcConnection> GetConnections()
    {
        lock (_connectionsLock)
        {
            return [.. _connections];
        }
    }

    public List<MoneroRpcConnection> GetRpcConnections()
    {
        lock (_connectionsLock)
        {
            return _connections.OfType<MoneroRpcConnection>().ToList();
        }
    }

    public MoneroConnectionManager RemoveConnection(MoneroRpcConnection connection)
    {
        lock (_connectionsLock)
        {
            _connections.Remove(connection);
            if (_currentConnection == connection)
            {
                _currentConnection = null;
                OnConnectionChanged(null);
            }
        }
        return this;
    }

    public MoneroConnectionManager RemoveConnection(string uri)
    {
        lock (_connectionsLock)
        {
            var connection = _connections.Find((connection => connection.GetUri() == uri));
            if (connection == null) return this; // no connection found
            _connections.Remove(connection);
            if (_currentConnection == connection)
            {
                _currentConnection = null;
                OnConnectionChanged(null);
            }
        }
        return this;
    }

    public MoneroConnectionManager CheckConnection()
    {
        bool connectionChanged = false;
        MoneroRpcConnection? connection = GetConnection();
        if (connection != null)
        {
            if (connection.CheckConnection(_timeoutMs)) connectionChanged = true;
            ProcessResponses([connection]);
        }
        if (_autoSwitch == true && IsConnected() == false)
        {
            MoneroRpcConnection bestConnection = GetBestAvailableConnection([connection]);
            if (bestConnection != null)
            {
                SetConnection(bestConnection);
                return this;
            }
        }
        if (connectionChanged) OnConnectionChanged(connection);
        return this;
    }

    public bool CheckConnections()
    {
        return CheckConnections(_connections, null);
    }

    public MoneroRpcConnection GetBestAvailableConnection(List<MoneroRpcConnection>? excludedConnections = null)
    {
        throw new NotImplementedException("This method is not implemented yet. Please implement the logic to get the best available connection excluding the specified connections.");
    }

    public MoneroConnectionManager Disconnect()
    {
        SetConnection(null);
        return this;
    }

    public MoneroConnectionManager Clear()
    {
        lock (_connectionsLock)
        {
            _connections.Clear();
            _currentConnection = null;
            OnConnectionChanged(null);
        }
        return this;
    }

    public MoneroConnectionManager Reset()
    {
        RemoveListeners();
        //StopPolling();
        Clear();
        _timeoutMs = DEFAULT_TIMEOUT;
        _autoSwitch = DEFAULT_AUTO_SWITCH;
        return this;
    }
}