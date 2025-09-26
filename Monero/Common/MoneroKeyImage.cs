using System.Text.Json.Serialization;

namespace Monero.Common;

public class MoneroKeyImage
{
    public enum SpentStatus
    {
        NotSpent,
        Confirmed,
        TxPool
    }

    [JsonPropertyName("key_image")]
    [JsonInclude]
    private string? _hex { get; set; }
    [JsonPropertyName("signature")]
    [JsonInclude]
    private string? _signature { get; set; }

    public MoneroKeyImage()
    {
    }

    public MoneroKeyImage(string? hex)
    {
        _hex = hex;
    }

    public MoneroKeyImage(string? hex, string? signature)
    {
        _hex = hex;
        _signature = signature;
    }

    private MoneroKeyImage(MoneroKeyImage keyImage)
    {
        _hex = keyImage._hex;
        _signature = keyImage._signature;
    }

    public MoneroKeyImage Clone()
    {
        return new MoneroKeyImage(this);
    }

    public string? GetHex()
    {
        return _hex;
    }

    public MoneroKeyImage SetHex(string? hex)
    {
        _hex = hex;
        return this;
    }

    public string? GetSignature()
    {
        return _signature;
    }
}