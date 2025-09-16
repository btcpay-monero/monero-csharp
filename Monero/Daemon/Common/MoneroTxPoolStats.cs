namespace Monero.Daemon.Common;

public class MoneroTxPoolStats
{
    private ulong? _bytesMax;
    private ulong? _bytesMed;
    private ulong? _bytesMin;
    private ulong? _bytesTotal;
    private ulong? _feeTotal;
    private Dictionary<ulong, int>? _histo;
    private ulong? _histo98Pc;
    private int? _num10M;
    private int? _numDoubleSpends;
    private int? _numFailing;
    private int? _numNotRelayed;
    private int? _numTxs;
    private ulong? _oldestTimestamp;

    public int? GetNumTxs()
    {
        return _numTxs;
    }

    public void SetNumTxs(int? numTxs)
    {
        this._numTxs = numTxs;
    }

    public int? GetNumNotRelayed()
    {
        return _numNotRelayed;
    }

    public void SetNumNotRelayed(int? numNotRelayed)
    {
        this._numNotRelayed = numNotRelayed;
    }

    public int? GetNumFailing()
    {
        return _numFailing;
    }

    public void SetNumFailing(int? numFailing)
    {
        this._numFailing = numFailing;
    }

    public int? GetNumDoubleSpends()
    {
        return _numDoubleSpends;
    }

    public void SetNumDoubleSpends(int? numDoubleSpends)
    {
        this._numDoubleSpends = numDoubleSpends;
    }

    public int? GetNum10M()
    {
        return _num10M;
    }

    public void SetNum10M(int? num10M)
    {
        this._num10M = num10M;
    }

    public ulong? GetFeeTotal()
    {
        return _feeTotal;
    }

    public void SetFeeTotal(ulong? feeTotal)
    {
        this._feeTotal = feeTotal;
    }

    public ulong? GetBytesMax()
    {
        return _bytesMax;
    }

    public void SetBytesMax(ulong? bytesMax)
    {
        this._bytesMax = bytesMax;
    }

    public ulong? GetBytesMed()
    {
        return _bytesMed;
    }

    public void SetBytesMed(ulong? bytesMed)
    {
        this._bytesMed = bytesMed;
    }

    public ulong? GetBytesMin()
    {
        return _bytesMin;
    }

    public void SetBytesMin(ulong? bytesMin)
    {
        this._bytesMin = bytesMin;
    }

    public ulong? GetBytesTotal()
    {
        return _bytesTotal;
    }

    public void SetBytesTotal(ulong? bytesTotal)
    {
        this._bytesTotal = bytesTotal;
    }

    public Dictionary<ulong, int>? GetHisto()
    {
        return _histo;
    }

    public void SetHisto(Dictionary<ulong, int> histo)
    {
        this._histo = histo;
    }

    public ulong? GetHisto98Pc()
    {
        return _histo98Pc;
    }

    public void SetHisto98Pc(ulong? histo98Pc)
    {
        this._histo98Pc = histo98Pc;
    }

    public ulong? GetOldestTimestamp()
    {
        return _oldestTimestamp;
    }

    public void SetOldestTimestamp(ulong? oldestTimestamp)
    {
        this._oldestTimestamp = oldestTimestamp;
    }
}