using Monero.Common;

namespace Monero.Daemon;

public class MoneroDaemonListener
{
    private MoneroBlockHeader? _lastHeader;

    public virtual void OnBlockHeader(MoneroBlockHeader header)
    {
        _lastHeader = header;
    }

    public MoneroBlockHeader? GetLastBlockHeader() { return _lastHeader; }
}