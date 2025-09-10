using Monero.Common;

namespace Monero.Wallet.Common;

public class MoneroWalletConfig
{
    private bool? _isMultisig;
    private MoneroNetworkType? _networkType;
    private string? _password;
    private string? _path;
    private MoneroRpcConnection? _server;
    private int? accountLookahead; // number of accounts to scan
    private byte[]? cacheData;
    private MoneroConnectionManager? connectionManager;
    private byte[]? keysData;
    private string? language;
    private string? primaryAddress;
    private string? privateSpendKey;
    private string? privateViewKey;
    private ulong? restoreHeight;
    private bool? saveCurrent;
    private string? seed;
    private string? seedOffset;
    private string? serverPassword;
    private string? serverUsername;
    private int? subaddressLookahead; // number of subaddresses to scan per account

    public MoneroWalletConfig() { }

    public MoneroWalletConfig(MoneroWalletConfig config)
    {
        _path = config.GetPath();
        _password = config.GetPassword();
        _networkType = config.GetNetworkType();
        _server = config.GetServer();
        connectionManager = config.GetConnectionManager();
        seed = config.GetSeed();
        seedOffset = config.GetSeedOffset();
        primaryAddress = config.GetPrimaryAddress();
        privateViewKey = config.GetPrivateViewKey();
        privateSpendKey = config.GetPrivateSpendKey();
        restoreHeight = config.GetRestoreHeight();
        language = config.GetLanguage();
        saveCurrent = config.GetSaveCurrent();
        accountLookahead = config.GetAccountLookahead();
        subaddressLookahead = config.GetSubaddressLookahead();
        keysData = config.GetKeysData();
        cacheData = config.GetCacheData();
        _isMultisig = config.IsMultisig();
    }

    public MoneroWalletConfig Clone()
    {
        return new MoneroWalletConfig(this);
    }

    public string? GetPath()
    {
        return _path;
    }

    public MoneroWalletConfig SetPath(string? path)
    {
        _path = path;
        return this;
    }

    public string? GetPassword()
    {
        return _password;
    }

    public MoneroWalletConfig SetPassword(string? password)
    {
        _password = password;
        return this;
    }

    public MoneroNetworkType? GetNetworkType()
    {
        return _networkType;
    }

    public MoneroWalletConfig SetNetworkType(MoneroNetworkType networkType)
    {
        _networkType = networkType;
        return this;
    }

    public MoneroWalletConfig SetNetworkType(string? networkTypeStr)
    {
        return SetNetworkType(MoneroNetwork.Parse(networkTypeStr));
    }

    public MoneroRpcConnection? GetServer()
    {
        return _server;
    }

    public MoneroWalletConfig SetServer(MoneroRpcConnection server)
    {
        _server = server;
        serverUsername = server == null ? null : server.GetUsername();
        serverPassword = server == null ? null : server.GetPassword();
        return this;
    }

    public string? GetServerUri()
    {
        return _server?.GetUri();
    }

    public MoneroWalletConfig SetServerUri(string? serverUri)
    {
        if (serverUri == null || serverUri == "")
        {
            _server = null;
            return this;
        }

        if (_server == null)
        {
            _server = new MoneroRpcConnection(serverUri);
        }
        else
        {
            _server.SetUri(serverUri);
        }

        if (serverUsername != null && serverPassword != null)
        {
            _server.SetCredentials(serverUsername, serverPassword);
        }

        return this;
    }

    public string? GetServerUsername()
    {
        return _server?.GetUsername();
    }

    public MoneroWalletConfig SetServerUsername(string? serverUsername)
    {
        this.serverUsername = serverUsername;
        if (_server != null && serverUsername != null && serverPassword != null)
        {
            _server.SetCredentials(serverUsername, serverPassword);
        }

        return this;
    }

    public string? GetServerPassword()
    {
        return _server?.GetPassword();
    }

    public MoneroWalletConfig SetServerPassword(string? serverPassword)
    {
        this.serverPassword = serverPassword;
        if (_server != null && serverUsername != null && serverPassword != null)
        {
            _server.SetCredentials(serverUsername, serverPassword);
        }

        return this;
    }

    public MoneroConnectionManager? GetConnectionManager()
    {
        return connectionManager;
    }

    public MoneroWalletConfig SetConnectionManager(MoneroConnectionManager connectionManager)
    {
        this.connectionManager = connectionManager;
        return this;
    }

    public string? GetSeed()
    {
        return seed;
    }

    public MoneroWalletConfig SetSeed(string? seed)
    {
        this.seed = seed;
        return this;
    }

    public string? GetSeedOffset()
    {
        return seedOffset;
    }

    public MoneroWalletConfig SetSeedOffset(string? seedOffset)
    {
        this.seedOffset = seedOffset;
        return this;
    }

    public string? GetPrimaryAddress()
    {
        return primaryAddress;
    }

    public MoneroWalletConfig SetPrimaryAddress(string? primaryAddress)
    {
        this.primaryAddress = primaryAddress;
        return this;
    }

    public string? GetPrivateViewKey()
    {
        return privateViewKey;
    }

    public MoneroWalletConfig SetPrivateViewKey(string? privateViewKey)
    {
        this.privateViewKey = privateViewKey;
        return this;
    }

    public string? GetPrivateSpendKey()
    {
        return privateSpendKey;
    }

    public MoneroWalletConfig SetPrivateSpendKey(string? privateSpendKey)
    {
        this.privateSpendKey = privateSpendKey;
        return this;
    }

    public ulong? GetRestoreHeight()
    {
        return restoreHeight;
    }

    public MoneroWalletConfig SetRestoreHeight(ulong? restoreHeight)
    {
        this.restoreHeight = restoreHeight;
        return this;
    }

    public string? GetLanguage()
    {
        return language;
    }

    public MoneroWalletConfig SetLanguage(string? language)
    {
        this.language = language;
        return this;
    }

    public bool? GetSaveCurrent()
    {
        return saveCurrent;
    }

    public MoneroWalletConfig SetSaveCurrent(bool? saveCurrent)
    {
        this.saveCurrent = saveCurrent;
        return this;
    }

    public MoneroWalletConfig SetAccountLookahead(int? accountLookahead)
    {
        this.accountLookahead = accountLookahead;
        return this;
    }

    public int? GetAccountLookahead()
    {
        return accountLookahead;
    }

    public MoneroWalletConfig SetSubaddressLookahead(int? subaddressLookahead)
    {
        this.subaddressLookahead = subaddressLookahead;
        return this;
    }

    public int? GetSubaddressLookahead()
    {
        return subaddressLookahead;
    }

    public byte[]? GetKeysData()
    {
        return keysData;
    }

    public MoneroWalletConfig SetKeysData(byte[]? keysData)
    {
        this.keysData = keysData;
        return this;
    }

    public byte[]? GetCacheData()
    {
        return cacheData;
    }

    public MoneroWalletConfig SetCacheData(byte[]? cacheData)
    {
        this.cacheData = cacheData;
        return this;
    }

    public bool? IsMultisig()
    {
        return _isMultisig;
    }

    public MoneroWalletConfig SetIsMultisig(bool? isMultisig)
    {
        _isMultisig = isMultisig;
        return this;
    }
}