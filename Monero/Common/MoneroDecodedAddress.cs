namespace Monero.Common;

public class MoneroDecodedAddress
{
    private string _address;
    private MoneroAddressType _addressType;
    private MoneroNetworkType _networkType;

    public MoneroDecodedAddress(string address, MoneroAddressType addressType, MoneroNetworkType networkType)
    {
        _address = address;
        _addressType = addressType;
        _networkType = networkType;
    }

    public MoneroDecodedAddress(MoneroDecodedAddress decodedAddress)
    {
        _address = decodedAddress._address;
        _addressType = decodedAddress._addressType;
        _networkType = decodedAddress._networkType;
    }

    public MoneroDecodedAddress Clone()
    {
        return new MoneroDecodedAddress(this);
    }

    public string GetAddress()
    {
        return _address;
    }

    public MoneroDecodedAddress SetAddress(string address)
    {
        _address = address;
        return this;
    }

    public MoneroAddressType GetAddressType()
    {
        return _addressType;
    }

    public MoneroDecodedAddress SetAddressType(MoneroAddressType addressType)
    {
        _addressType = addressType;
        return this;
    }

    public MoneroNetworkType GetNetworkType()
    {
        return _networkType;
    }

    public MoneroDecodedAddress SetNetworkType(MoneroNetworkType networkType)
    {
        _networkType = networkType;
        return this;
    }
}