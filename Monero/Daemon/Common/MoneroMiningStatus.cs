using System.Text.Json.Serialization;

namespace Monero.Daemon.Common;

public class MoneroMiningStatus : MoneroRpcResponse
{
    [JsonPropertyName("address")]
    [JsonInclude]
    private string? _address { get; set; }
    [JsonPropertyName("active")]
    [JsonInclude]
    private bool? _isActive { get; set; }
    [JsonPropertyName("is_background_mining_enabled")]
    [JsonInclude]
    private bool? _isBackground { get; set; }
    [JsonPropertyName("threads_count")]
    [JsonInclude]
    private uint? _numThreads { get; set; }
    [JsonPropertyName("speed")]
    [JsonInclude]
    private ulong? _speed { get; set; }

    public bool? IsActive()
    {
        return _isActive;
    }

    public void SetIsActive(bool? isActive)
    {
        _isActive = isActive;
    }

    public bool? IsBackground()
    {
        return _isBackground;
    }

    public void SetIsBackground(bool? isBackground)
    {
        _isBackground = isBackground;
    }

    public string? GetAddress()
    {
        return _address;
    }

    public void SetAddress(string? address)
    {
        _address = address;
    }

    public ulong? GetSpeed()
    {
        return _speed;
    }

    public void SetSpeed(ulong? speed)
    {
        _speed = speed;
    }

    public uint? GetNumThreads()
    {
        return _numThreads;
    }

    public void SetNumThreads(uint? numThreads)
    {
        _numThreads = numThreads;
    }
}