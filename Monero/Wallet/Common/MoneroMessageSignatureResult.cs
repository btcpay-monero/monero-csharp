
namespace Monero.Wallet.Common;

public class MoneroMessageSignatureResult
{
    private bool _isGood;
    private bool? _isOld;
    private MoneroMessageSignatureType? _signatureType;
    private int? _version;

    public MoneroMessageSignatureResult(bool isGood = false, bool? isOld = null, MoneroMessageSignatureType? signatureType = null, int? version = null)
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

    public MoneroMessageSignatureType? GetSignatureType()
    {
        return _signatureType;
    }

    public MoneroMessageSignatureResult SetSignatureType(MoneroMessageSignatureType? signatureType)
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