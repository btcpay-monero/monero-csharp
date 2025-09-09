
namespace Monero.Common;

public class MoneroIntegratedAddress
{
    private string? _standardAddress;
    private string? _paymentId;
    private string? _integratedAddress;

    public MoneroIntegratedAddress(string? standardAddress = null, string? paymentId = null, string? integratedAddress = null)
    {
        _standardAddress = standardAddress;
        _paymentId = paymentId;
        _integratedAddress = integratedAddress;
    }

    public MoneroIntegratedAddress(MoneroIntegratedAddress integratedAddress)
    {
        _standardAddress = integratedAddress._standardAddress;
        _paymentId = integratedAddress._paymentId;
        _integratedAddress = integratedAddress._integratedAddress;
    }

    public MoneroIntegratedAddress Clone()
    {
        return new MoneroIntegratedAddress(this);
    }

    public string? GetStandardAddress()
    {
        return _standardAddress;
    }

    public MoneroIntegratedAddress SetStandardAddress(string? standardAddress)
    {
        _standardAddress = standardAddress;
        return this;
    }

    public string? GetPaymentId()
    {
        return _paymentId;
    }

    public MoneroIntegratedAddress SetPaymentId(string? paymentId)
    {
        _paymentId = paymentId;
        return this;
    }

    public string? GetIntegratedAddress()
    {
        return _integratedAddress;
    }

    public MoneroIntegratedAddress SetIntegratedAddress(string? integratedAddress)
    {
        _integratedAddress = integratedAddress;
        return this;
    }
}