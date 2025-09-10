namespace Monero.Daemon.Common;

public class MoneroTxPoolStats
{
    private ulong? bytesMax;
    private ulong? bytesMed;
    private ulong? bytesMin;
    private ulong? bytesTotal;
    private ulong? feeTotal;
    private Dictionary<ulong, int>? histo;
    private ulong? histo98pc;
    private int? num10m;
    private int? numDoubleSpends;
    private int? numFailing;
    private int? numNotRelayed;
    private int? numTxs;
    private ulong? oldestTimestamp;

    public int? GetNumTxs()
    {
        return numTxs;
    }

    public void SetNumTxs(int? numTxs)
    {
        this.numTxs = numTxs;
    }

    public int? GetNumNotRelayed()
    {
        return numNotRelayed;
    }

    public void SetNumNotRelayed(int? numNotRelayed)
    {
        this.numNotRelayed = numNotRelayed;
    }

    public int? GetNumFailing()
    {
        return numFailing;
    }

    public void SetNumFailing(int? numFailing)
    {
        this.numFailing = numFailing;
    }

    public int? GetNumDoubleSpends()
    {
        return numDoubleSpends;
    }

    public void SetNumDoubleSpends(int? numDoubleSpends)
    {
        this.numDoubleSpends = numDoubleSpends;
    }

    public int? GetNum10m()
    {
        return num10m;
    }

    public void SetNum10m(int? num10m)
    {
        this.num10m = num10m;
    }

    public ulong? GetFeeTotal()
    {
        return feeTotal;
    }

    public void SetFeeTotal(ulong? feeTotal)
    {
        this.feeTotal = feeTotal;
    }

    public ulong? GetBytesMax()
    {
        return bytesMax;
    }

    public void SetBytesMax(ulong? bytesMax)
    {
        this.bytesMax = bytesMax;
    }

    public ulong? GetBytesMed()
    {
        return bytesMed;
    }

    public void SetBytesMed(ulong? bytesMed)
    {
        this.bytesMed = bytesMed;
    }

    public ulong? GetBytesMin()
    {
        return bytesMin;
    }

    public void SetBytesMin(ulong? bytesMin)
    {
        this.bytesMin = bytesMin;
    }

    public ulong? GetBytesTotal()
    {
        return bytesTotal;
    }

    public void SetBytesTotal(ulong? bytesTotal)
    {
        this.bytesTotal = bytesTotal;
    }

    public Dictionary<ulong, int>? GetHisto()
    {
        return histo;
    }

    public void SetHisto(Dictionary<ulong, int> histo)
    {
        this.histo = histo;
    }

    public ulong? GetHisto98pc()
    {
        return histo98pc;
    }

    public void SetHisto98pc(ulong? histo98pc)
    {
        this.histo98pc = histo98pc;
    }

    public ulong? GetOldestTimestamp()
    {
        return oldestTimestamp;
    }

    public void SetOldestTimestamp(ulong? oldestTimestamp)
    {
        this.oldestTimestamp = oldestTimestamp;
    }
}