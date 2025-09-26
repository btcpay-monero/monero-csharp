using System.Text.Json.Serialization;

namespace Monero.Daemon.Common;

public class MoneroTxPoolStats : MoneroRpcResponse
{
    [JsonPropertyName("bytes_max")]
    [JsonInclude]
    private ulong? _bytesMax { get; set; }
    [JsonPropertyName("bytes_med")]
    [JsonInclude]
    private ulong? _bytesMed { get; set; }
    [JsonPropertyName("bytes_min")]
    [JsonInclude]
    private ulong? _bytesMin { get; set; }
    [JsonPropertyName("bytes_total")]
    [JsonInclude]
    private ulong? _bytesTotal { get; set; }
    [JsonPropertyName("fee_total")]
    [JsonInclude]
    private ulong? _feeTotal { get; set; }
    [JsonPropertyName("histo")]
    [JsonInclude]
    private Dictionary<ulong, int>? _histo { get; set; }
    [JsonPropertyName("histo_98pc")]
    [JsonInclude]
    private ulong? _histo98Pc { get; set; }
    [JsonPropertyName("num_10m")]
    [JsonInclude]
    private int? _num10M { get; set; }
    [JsonPropertyName("num_double_spends")]
    [JsonInclude]
    private int? _numDoubleSpends { get; set; }
    [JsonPropertyName("num_failing")]
    [JsonInclude]
    private int? _numFailing { get; set; }
    [JsonPropertyName("num_not_relayed")]
    [JsonInclude]
    private int? _numNotRelayed { get; set; }
    [JsonPropertyName("txs_total")]
    [JsonInclude]
    private int? _numTxs { get; set; }
    [JsonPropertyName("oldest")]
    [JsonInclude]
    private ulong? _oldestTimestamp { get; set; }

    public int? GetNumTxs()
    {
        return _numTxs;
    }

    public void SetNumTxs(int? numTxs)
    {
        _numTxs = numTxs;
    }

    public int? GetNumNotRelayed()
    {
        return _numNotRelayed;
    }

    public void SetNumNotRelayed(int? numNotRelayed)
    {
        _numNotRelayed = numNotRelayed;
    }

    public int? GetNumFailing()
    {
        return _numFailing;
    }

    public void SetNumFailing(int? numFailing)
    {
        _numFailing = numFailing;
    }

    public int? GetNumDoubleSpends()
    {
        return _numDoubleSpends;
    }

    public void SetNumDoubleSpends(int? numDoubleSpends)
    {
        _numDoubleSpends = numDoubleSpends;
    }

    public int? GetNum10M()
    {
        return _num10M;
    }

    public void SetNum10M(int? num10M)
    {
        _num10M = num10M;
    }

    public ulong? GetFeeTotal()
    {
        return _feeTotal;
    }

    public void SetFeeTotal(ulong? feeTotal)
    {
        _feeTotal = feeTotal;
    }

    public ulong? GetBytesMax()
    {
        return _bytesMax;
    }

    public void SetBytesMax(ulong? bytesMax)
    {
        _bytesMax = bytesMax;
    }

    public ulong? GetBytesMed()
    {
        return _bytesMed;
    }

    public void SetBytesMed(ulong? bytesMed)
    {
        _bytesMed = bytesMed;
    }

    public ulong? GetBytesMin()
    {
        return _bytesMin;
    }

    public void SetBytesMin(ulong? bytesMin)
    {
        _bytesMin = bytesMin;
    }

    public ulong? GetBytesTotal()
    {
        return _bytesTotal;
    }

    public void SetBytesTotal(ulong? bytesTotal)
    {
        _bytesTotal = bytesTotal;
    }

    public Dictionary<ulong, int>? GetHisto()
    {
        return _histo;
    }

    public void SetHisto(Dictionary<ulong, int> histo)
    {
        _histo = histo;
    }

    public ulong? GetHisto98Pc()
    {
        return _histo98Pc;
    }

    public void SetHisto98Pc(ulong? histo98Pc)
    {
        _histo98Pc = histo98Pc;
    }

    public ulong? GetOldestTimestamp()
    {
        return _oldestTimestamp;
    }

    public void SetOldestTimestamp(ulong? oldestTimestamp)
    {
        _oldestTimestamp = oldestTimestamp;
    }
}