using Monero.Common;

namespace Monero.Daemon;

public class MoneroDaemonPoller
{
    private const ulong DefaultPollPeriodInMs = 10000; // Poll every X ms

    private readonly MoneroDaemonRpc _daemon;
    private readonly TaskLooper _looper;
    private MoneroBlockHeader? _lastHeader;

    public MoneroDaemonPoller(MoneroDaemonRpc daemon)
    {
        this._daemon = daemon;
        _looper = new TaskLooper(async () => await Poll());
    }

    public void SetIsPolling(bool isPolling)
    {
        if (isPolling)
        {
            _looper.Start(DefaultPollPeriodInMs); // TODO: allow configurable Poll period
        }
        else
        {
            _looper.Stop();
        }
    }

    private async Task Poll()
    {
        try
        {
            // get first header for comparison
            if (_lastHeader == null)
            {
                _lastHeader = await _daemon.GetLastBlockHeader();
                return;
            }

            // fetch and compare the latest block header
            MoneroBlockHeader header = await _daemon.GetLastBlockHeader();
            string? headerHash = header.GetHash();
            if (headerHash != null && !headerHash.Equals(_lastHeader.GetHash()))
            {
                _lastHeader = header;
                lock (_daemon.GetListeners())
                {
                    AnnounceBlockHeader(header);
                }
            }
        }
        catch (Exception e)
        {
            MoneroUtils.Log(0, $"[daemon-poller] error: {e.Message}");
        }
    }

    private void AnnounceBlockHeader(MoneroBlockHeader header)
    {
        foreach (MoneroDaemonListener listener in _daemon.GetListeners())
        {
            try
            {
                listener.OnBlockHeader(header);
            }
            catch (Exception e)
            {
                MoneroUtils.Log(0, "Error calling listener on new block header: " + e.Message);
            }
        }
    }
}