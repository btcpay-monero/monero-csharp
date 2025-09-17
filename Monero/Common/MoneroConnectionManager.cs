using System.Collections.ObjectModel;

namespace Monero.Common;

public class MoneroConnectionManager
{
    public enum PollType
    {
        Prioritized,
        Current,
        All
    }

    private static readonly ulong DefaultTimeout = 5000;
    private static readonly ulong DefaultPollPeriod = 20000;
    private static readonly bool DefaultAutoSwitch = true;
    private static readonly int MinBetterResponses = 3;
    private readonly List<MoneroRpcConnection> _connections = [];
    private readonly object _connectionsLock = new();

    private readonly List<MoneroConnectionManagerListener> _listeners = [];
    private readonly object _listenersLock = new();

    private readonly Dictionary<MoneroRpcConnection, List<ulong?>> _responseTimes = [];

    private bool _autoSwitch = DefaultAutoSwitch;

    private MoneroRpcConnection? _currentConnection;

    private ulong _timeoutMs = DefaultTimeout;

    private TaskLooper? _poller;

    private void OnConnectionChanged(MoneroRpcConnection? connection)
    {
        lock (_listenersLock)
        {
            foreach (MoneroConnectionManagerListener listener in _listeners)
            {
                listener.OnConnectionChanged(connection);
            }
        }
    }

    private MoneroRpcConnection? GetBestConnectionFromPrioritizedResponses(List<MoneroRpcConnection>? responses)
    {
        if (responses == null)
        {
            return null;
        }
        // get best response
        MoneroRpcConnection? bestResponse = null;

        foreach (MoneroRpcConnection connection in responses)
        {
            if (connection.IsConnected() == true &&
                (bestResponse == null || connection.GetResponseTime() < bestResponse.GetResponseTime()))
            {
                bestResponse = connection;
            }
        }

        // no update if no responses
        if (bestResponse == null)
        {
            return null;
        }

        // use best response if disconnected
        MoneroRpcConnection? bestConnection = GetConnection();
        if (bestConnection == null || bestConnection.IsConnected() != true)
        {
            return bestResponse;
        }

        ConnectionPriorityComparator priorityComparator = new();

        // use best response if different priority (assumes being called in descending priority)
        if (priorityComparator.Compare(bestResponse.GetPriority(), bestConnection.GetPriority()) != 0)
        {
            return bestResponse;
        }

        // keep best connection if not enough data
        if (!_responseTimes.ContainsKey(bestConnection))
        {
            return bestConnection;
        }

        // check if a connection is consistently better
        foreach (MoneroRpcConnection connection in responses)
        {
            if (connection == bestConnection)
            {
                continue;
            }

            if (!_responseTimes.ContainsKey(connection) ||
                _responseTimes[connection].Count < MinBetterResponses)
            {
                continue;
            }

            bool better = true;
            for (int i = 0; i < MinBetterResponses; i++)
            {
                if (_responseTimes[connection][i] == null || _responseTimes[bestConnection][i] == null ||
                    _responseTimes[connection][i] > _responseTimes[bestConnection][i])
                {
                    better = false;
                    break;
                }
            }

            if (better)
            {
                bestConnection = connection;
            }
        }

        return bestConnection;
    }

    private MoneroRpcConnection? UpdateBestConnectionInPriority()
    {
        if (!_autoSwitch)
        {
            return null;
        }

        foreach (List<MoneroRpcConnection> prioritizedConnections in GetConnectionsInAscendingPriority())
        {
            MoneroRpcConnection? bestConnectionFromResponses =
                GetBestConnectionFromPrioritizedResponses(prioritizedConnections);
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
            if (!_responseTimes.ContainsKey(connection))
            {
                _responseTimes.Add(connection, []);
            }
        }

        // insert response times or null
        foreach (KeyValuePair<MoneroRpcConnection, List<ulong?>> responseTime in _responseTimes.ToList())
        {
            responseTime.Value.Add(0);
            responseTime.Value.Add(responses.Contains(responseTime.Key) ? responseTime.Key.GetResponseTime() : null);

            // remove old response times
            if (responseTime.Value.Count > MinBetterResponses)
            {
                responseTime.Value.RemoveAt(responseTime.Value.Count - 1);
            }
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
                if (!connectionPriorities.ContainsKey(connection.GetPriority()))
                {
                    connectionPriorities.Add(connection.GetPriority(), []);
                }

                connectionPriorities[connection.GetPriority()].Add(connection);
            }

            List<List<MoneroRpcConnection>> prioritizedConnections = [];
            foreach (List<MoneroRpcConnection> priorityConnections in connectionPriorities.Values)
            {
                prioritizedConnections.Add(priorityConnections);
            }

            if (connectionPriorities.ContainsKey(0))
            {
                List<MoneroRpcConnection> first = prioritizedConnections[0]; // move priority 0 to end
                prioritizedConnections.RemoveAt(0);
                prioritizedConnections.Add(first); // move priority 0 to end
            }

            return prioritizedConnections;
        }
    }

    private Task<bool> CheckConnections(List<MoneroRpcConnection> connections, List<MoneroRpcConnection>? excludedConnections)
    {
        throw new NotImplementedException("");
    }

    private async Task CheckPrioritizedConnections(List<MoneroRpcConnection>? excludedConnections)
    {
        foreach (List<MoneroRpcConnection> prioritizedConnections in GetConnectionsInAscendingPriority())
        {
            bool hasConnection = await CheckConnections(prioritizedConnections, excludedConnections);
            if (hasConnection)
            {
                return;
            }
        }
    }

    private void StartPollingConnection(ulong periodMs)
    {
        _poller = new TaskLooper(async () =>
        {
            try { await CheckConnection(); }
            catch (Exception e) { MoneroUtils.Log(0, e.StackTrace != null ? e.StackTrace : ""); }
        });
        _poller.Start(periodMs);
    }

    private void StartPollingConnections(ulong periodMs)
    {
        _poller = new TaskLooper(async () =>
        {
            try { await CheckConnections(); }
            catch (Exception e) { MoneroUtils.Log(0, e.StackTrace != null ? e.StackTrace : ""); }
        });
        _poller.Start(periodMs);
    }

    private void StartPollingPrioritizedConnections(ulong periodMs)
    {
        StartPollingPrioritizedConnections(periodMs, null);
    }

    private void StartPollingPrioritizedConnections(ulong periodMs,
        List<MoneroRpcConnection>? excludedConnections)
    {
        _poller = new TaskLooper(async () =>
        {
            try { await CheckPrioritizedConnections(excludedConnections); }
            catch (Exception e) { MoneroUtils.Log(0, e.StackTrace != null ? e.StackTrace : ""); }
        });
        _poller.Start(periodMs);
    }

    public MoneroConnectionManager StartPolling()
    {
        return StartPolling(null, null, null, null, null);
    }

    public MoneroConnectionManager StartPolling(ulong? periodMs)
    {
        return StartPolling(periodMs, null, null, null, null);
    }

    public MoneroConnectionManager StartPolling(ulong? periodMs, bool? autoSwitch)
    {
        return StartPolling(periodMs, autoSwitch, null, null, null);
    }

    public MoneroConnectionManager StartPolling(ulong? periodMs, bool? autoSwitch, ulong? timeoutMs)
    {
        return StartPolling(periodMs, autoSwitch, timeoutMs, null, null);
    }

    public MoneroConnectionManager StartPolling(ulong? periodMs, bool? autoSwitch,
        ulong? timeoutMs, PollType? pollType)
    {
        return StartPolling(periodMs, autoSwitch, timeoutMs, pollType, null);
    }

    public MoneroConnectionManager StartPolling(ulong? periodMs, bool? autoSwitch,
        ulong? timeoutMs, PollType? pollType, List<MoneroRpcConnection>? excludedConnections)
    {
        // apply defaults
        if (periodMs == null)
        {
            periodMs = DefaultPollPeriod;
        }

        if (autoSwitch != null)
        {
            SetAutoSwitch((bool)autoSwitch);
        }

        if (timeoutMs != null)
        {
            SetTimeout(timeoutMs);
        }

        if (pollType == null)
        {
            pollType = PollType.Prioritized;
        }

        // stop polling
        StopPolling();

        // start polling
        switch (pollType)
        {
            case PollType.Current:
                StartPollingConnection((ulong)periodMs);
                break;
            case PollType.All:
                StartPollingConnections((ulong)periodMs);
                break;
            default:
                StartPollingPrioritizedConnections((ulong)periodMs, excludedConnections);
                break;
        }

        return this;
    }

    public MoneroConnectionManager StopPolling()
    {
        if (_poller != null)
        {
            _poller.Stop();
        }

        _poller = null;
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
            _timeoutMs = DefaultTimeout;
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
            if (_currentConnection == null)
            {
                return false;
            }

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

    public MoneroConnectionManager AddRpcConnection(string uri)
    {
        return AddRpcConnection(uri, null, null, null);
    }

    public MoneroConnectionManager AddRpcConnection(string uri, string? username, string? password)
    {
        return AddRpcConnection(uri, username, password, null);
    }

    public MoneroConnectionManager AddRpcConnection(string uri, string? username, string? password, string? zmqUri)
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
            if (_currentConnection == connection)
            {
                return this;
            }

            if (connection == null)
            {
                _currentConnection = null;
                OnConnectionChanged(null);
                return this;
            }

            string? uri = connection.GetUri();

            if (string.IsNullOrEmpty(uri))
            {
                throw new MoneroError("Connection is missing URI");
            }

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

    public MoneroConnectionManager SetConnection(string uri)
    {
        return SetConnection(new MoneroRpcConnection(uri, null, null, null));
    }

    public MoneroConnectionManager SetConnection(string uri, string? username, string? password)
    {
        return SetConnection(new MoneroRpcConnection(uri, username, password, null));
    }

    public MoneroConnectionManager SetConnection(string uri, string? username, string? password, string? zmqUri)
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

    public MoneroConnectionManager RemoveConnection(string? uri)
    {
        if (uri == null)
        {
            throw new MoneroError("Uri is null");
        }

        lock (_connectionsLock)
        {
            MoneroRpcConnection? connection = _connections.Find(connection => connection.GetUri() == uri);
            if (connection == null)
            {
                return this; // no connection found
            }

            _connections.Remove(connection);
            if (_currentConnection == connection)
            {
                _currentConnection = null;
                OnConnectionChanged(null);
            }
        }

        return this;
    }

    public async Task<MoneroConnectionManager> CheckConnection()
    {
        bool connectionChanged = false;
        MoneroRpcConnection? connection = GetConnection();
        if (connection != null)
        {
            if (await connection.CheckConnection(_timeoutMs))
            {
                connectionChanged = true;
            }

            ProcessResponses([connection]);
        }

        if (_autoSwitch && IsConnected() == false)
        {
            List<MoneroRpcConnection> excludedConnections = connection == null ? [] : [connection];
            MoneroRpcConnection? bestConnection = GetBestAvailableConnection(excludedConnections);
            if (bestConnection != null)
            {
                SetConnection(bestConnection);
                return this;
            }
        }

        if (connectionChanged)
        {
            OnConnectionChanged(connection);
        }

        return this;
    }

    public async Task<bool> CheckConnections()
    {
        return await CheckConnections(_connections, null);
    }

    public MoneroRpcConnection? GetBestAvailableConnection()
    {
        return GetBestAvailableConnection(null);
    }

    public MoneroRpcConnection? GetBestAvailableConnection(List<MoneroRpcConnection>? excludedConnections)
    {
        throw new NotImplementedException(
            "This method is not implemented yet. Please implement the logic to get the best available connection excluding the specified connections.");
    }

    public MoneroConnectionManager Disconnect()
    {
        SetConnection((MoneroRpcConnection?)null);
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
        Clear();
        _timeoutMs = DefaultTimeout;
        _autoSwitch = DefaultAutoSwitch;
        return this;
    }

    private class ConnectionPriorityComparator : IComparer<int>
    {
        public int Compare(int p1, int p2)
        {
            if (p1 == p2)
            {
                return 0;
            }

            if (p1 == 0)
            {
                return -1;
            }

            if (p2 == 0)
            {
                return 1;
            }

            return p2 - p1;
        }
    }

    private class ConnectionComparator : Comparer<MoneroRpcConnection>
    {
        public readonly ConnectionPriorityComparator PriorityComparator = new();
        public MoneroRpcConnection? CurrentConnection = null;

        public override int Compare(MoneroRpcConnection? c1, MoneroRpcConnection? c2)
        {
            // current connection is first
            if (c1 == CurrentConnection)
            {
                return -1;
            }

            if (c2 == CurrentConnection)
            {
                return 1;
            }

            if (c1 == null && c2 == null)
            {
                return 0;
            }

            if (c1 == null)
            {
                return 1;
            }

            if (c2 == null)
            {
                return -1;
            }

            // order by availability then priority then by name
            if (c1.IsOnline() == c2.IsOnline())
            {
                if (c1.GetPriority() == c2.GetPriority())
                {
                    string c1Uri = c1.GetUri() ?? "";
                    string c2Uri = c2.GetUri() ?? "";
                    return String.Compare(c1Uri, c2Uri, StringComparison.Ordinal);
                }

                return PriorityComparator.Compare(c1.GetPriority(), c2.GetPriority()) *
                       -1; // order by priority in descending order
            }

            if (c1.IsOnline() == true)
            {
                return -1;
            }

            if (c2.IsOnline() == true)
            {
                return 1;
            }

            if (c1.IsOnline() == null)
            {
                return -1;
            }

            return 1; // c1 is offline
        }
    }
}