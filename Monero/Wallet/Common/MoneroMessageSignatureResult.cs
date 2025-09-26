using System.Text.Json.Serialization;

namespace Monero.Wallet.Common;

public class MoneroMessageSignatureResult
{
    [JsonPropertyName("good")]
    private bool _isGood;
    [JsonPropertyName("old")]
    private bool? _isOld;
    [JsonPropertyName("signature_type")]
    private string? _signatureType;
    [JsonPropertyName("version")]
    private int? _version;

    public MoneroMessageSignatureResult()
    {
        _isGood = false;
    }

    public MoneroMessageSignatureResult(bool isGood)
    {
        _isGood = isGood;
    }

    public MoneroMessageSignatureResult(bool isGood, bool? isOld)
    {
        _isGood = isGood;
        _isOld = isOld;
    }

    public MoneroMessageSignatureResult(bool isGood, bool? isOld,
        string? signatureType)
    {
        _isGood = isGood;
        _isOld = isOld;
        _signatureType = signatureType;
    }

    public MoneroMessageSignatureResult(bool isGood, bool? isOld,
        string? signatureType, int? version)
    {
        _isGood = isGood;
        _isOld = isOld;
        _signatureType = signatureType;
        _version = version;
    }

    public MoneroMessageSignatureResult(MoneroMessageSignatureResult signatureResult)
    {
        _isGood = signatureResult._isGood;
        _isOld = signatureResult._isOld;
        _signatureType = signatureResult._signatureType;
        _version = signatureResult._version;
    }

    public bool IsGood()
    {
        return _isGood;
    }

    public MoneroMessageSignatureResult SetIsGood(bool isGood)
    {
        _isGood = isGood;
        return this;
    }

    public bool? IsOld()
    {
        return _isOld;
    }

    public MoneroMessageSignatureResult SetIsOld(bool? isOld)
    {
        _isOld = isOld;
        return this;
    }

    public string? GetSignatureType()
    {
        return _signatureType;
    }

    public MoneroMessageSignatureResult SetSignatureType(string? signatureType)
    {
        _signatureType = signatureType;
        return this;
    }

    public int? GetVersion()
    {
        return _version;
    }

    public MoneroMessageSignatureResult SetVersion(int? version)
    {
        _version = version;
        return this;
    }

    public MoneroMessageSignatureResult Clone()
    {
        return new MoneroMessageSignatureResult(this);
    }
}