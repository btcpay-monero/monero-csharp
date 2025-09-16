using Monero.Common;

namespace Monero.Daemon.Common;

public class MoneroDaemonConfig
{
    private string? _path;
    private bool? _allowLocalIp;
    private string? _banList;
    private int? _blockSyncSize;
    private string? _booststrapDaemonLogin;
    private string? _bootstrapDaemonAddress;
    private bool? _confirmExternalBind;

    // Server
    private string? _dataDir;
    private string? _dbSyncMode;
    private bool? _detach;
    private bool? _disableDnsCheckpoints;
    private bool? _disableRpcBan;

    private bool? _enableDnsBlocklist;
    private bool? _enforceDnsCheckpointing;
    private List<string> _exclusiveNodes = [];
    private bool? _fastBlockSync;
    private bool? _hideMyPort;
    private string? _i2PAnonymousInbound;
    private string? _i2PTxProxy;
    private string? _igd;
    private List<string> _inPeers = [];
    private int? _limitRate;
    private int? _limitRateDown;
    private int? _limitRateUp;

    // Log
    private string? _logFile;
    private int? _logLevel;
    private int? _maxConcurrency;

    private int? _maxConnectionsPerIp;

    private int? _maxLogFiles;
    private int? _maxLogFileSize;
    private ulong? _maxTxPoolWeight;

    // Network type
    private MoneroNetworkType _networkType = MoneroNetworkType.Mainnet;
    private bool? _noIgd;
    private bool? _nonInteractive;
    private bool? _noSync;
    private bool? _noZmq;
    private bool? _offline;
    private List<string> _outPeers = [];

    // P2P Network
    private string? _p2PBindIp;
    private string? _p2PBindIpv6Address;
    private int? _p2PBindPort;
    private int? _p2PBindPortIpv6;
    private int? _p2PExternalPort;
    private bool? _p2PIgnoreIpv4;
    private bool? _p2PUseIpv6;
    private bool? _padTransactions;
    private List<string> _peers = [];
    private string? _pidFile;
    private int? _prepBlocksThreads;
    private List<string> _priorityNodes = [];

    private string? _proxy;

    // Performance
    private bool? _pruneBlockchain;

    // Node RPC API
    private bool? _publicNode;

    private bool? _restrictedRpc;
    private string? _rpcAccessControlOrigins;
    private string? _rpcBindIp;
    private string? _rpcBindIpv6Address;
    private int? _rpcBindPort;
    private bool? _rpcIgnoreIpv4;
    private string? _rpcLogin;
    private int? _rpcMacConnectionsPerPrivateIp;
    private int? _rpcMaxConnections;
    private int? _rpcMaxConnectionsPerPublicIp;
    private int? _rpcMaxResponseSoftLimit;
    private string? _rpcRestrictedBindIp;
    private string? _rpcRestrictedBindIpv6Address;
    private int? _rpcRestrictedBindPort;
    private string? _rpcSsl;
    private bool? _rpcSslAllowAnyCert;
    private bool? _rpcSslAllowChained;
    private List<string> _rpcSslAllowedFingerprints = [];
    private string? _rpcSslCaCertificates;
    private string? _rpcSslCertificate;
    private string? _rpcSslPrivateKey;
    private bool? _rpcUseIpv6;
    private string? _seedNode;
    private bool? _syncPrunedBlocks;
    private string? _torAnonymousInbound;

    // Tor/I2P and proxies
    private string? _torTxProxy;
    private string? _zmqPub;
    private string? _zmqRpcBindIp;
    private int? _zmqRpcBindPort;

    public string? GetPath()
    {
        return _path;
    }

    public MoneroDaemonConfig SetPath(string? path)
    {
        this._path = path;
        return this;
    }

    public MoneroDaemonConfig SetNetworkType(MoneroNetworkType networkType)
    {
        this._networkType = networkType;
        return this;
    }

    public MoneroNetworkType GetNetworkType()
    {
        return _networkType;
    }

    public MoneroDaemonConfig SetLogFile(string? logFile)
    {
        this._logFile = logFile;
        return this;
    }

    public string? GetLogFile()
    {
        return _logFile;
    }

    public MoneroDaemonConfig SetLogLevel(int? logLevel)
    {
        this._logLevel = logLevel;
        return this;
    }

    public int? GetLogLevel()
    {
        return _logLevel;
    }

    public MoneroDaemonConfig SetMaxLogFileSize(int? maxLogFileSize)
    {
        this._maxLogFileSize = maxLogFileSize;
        return this;
    }

    public int? GetMaxLogFileSize()
    {
        return _maxLogFileSize;
    }

    public MoneroDaemonConfig SetMaxLogFiles(int? maxLogFiles)
    {
        this._maxLogFiles = maxLogFiles;
        return this;
    }

    public int? GetMaxLogFiles()
    {
        return _maxLogFiles;
    }

    public MoneroDaemonConfig SetDataDir(string? dataDir)
    {
        this._dataDir = dataDir;
        return this;
    }

    public string? GetDataDir()
    {
        return _dataDir;
    }

    public MoneroDaemonConfig SetPidFile(string? pidFile)
    {
        this._pidFile = pidFile;
        return this;
    }

    public string? GetPidFile()
    {
        return _pidFile;
    }

    public MoneroDaemonConfig SetDetach(bool? detach)
    {
        this._detach = detach;
        return this;
    }

    public bool? GetDetach()
    {
        return _detach;
    }

    public MoneroDaemonConfig SetNonInteractive(bool? nonInteractive)
    {
        this._nonInteractive = nonInteractive;
        return this;
    }

    public bool? GetNonInteractive()
    {
        return _nonInteractive;
    }

    public MoneroDaemonConfig SetMaxTxPoolWeight(ulong? maxTxPoolWeight)
    {
        this._maxTxPoolWeight = maxTxPoolWeight;
        return this;
    }

    public ulong? GetMaxTxPoolWeight()
    {
        return _maxTxPoolWeight;
    }

    public MoneroDaemonConfig SetEnforceDnsCheckpointing(bool? enforceDnsCheckpointing)
    {
        this._enforceDnsCheckpointing = enforceDnsCheckpointing;
        return this;
    }

    public bool? GetEnforceDnsCheckpointing()
    {
        return _enforceDnsCheckpointing;
    }

    public MoneroDaemonConfig SetDisableDnsCheckpoints(bool? disableDnsCheckpoints)
    {
        this._disableDnsCheckpoints = disableDnsCheckpoints;
        return this;
    }

    public bool? GetDisableDnsCheckpoints()
    {
        return _disableDnsCheckpoints;
    }

    public MoneroDaemonConfig SetBanList(string? banList)
    {
        this._banList = banList;
        return this;
    }

    public string? GetBanList()
    {
        return _banList;
    }

    public MoneroDaemonConfig SetEnabledDnsBlocklist(bool? enabledDnsBlocklist)
    {
        _enableDnsBlocklist = enabledDnsBlocklist;
        return this;
    }

    public bool? GetEnabledDnsBlocklist()
    {
        return _enableDnsBlocklist;
    }

    public MoneroDaemonConfig SetP2pBindIp(string? p2pBindIp)
    {
        this._p2PBindIp = p2pBindIp;
        return this;
    }

    public string? GetP2pBindIp()
    {
        return _p2PBindIp;
    }

    public MoneroDaemonConfig SetP2pBindPort(int? p2pBindPort)
    {
        this._p2PBindPort = p2pBindPort;
        return this;
    }

    public int? GetP2pBindPort()
    {
        return _p2PBindPort;
    }

    public MoneroDaemonConfig SetP2pExternalPort(int? p2pExternalPort)
    {
        this._p2PExternalPort = p2pExternalPort;
        return this;
    }

    public int? GetP2pExternalPort()
    {
        return _p2PExternalPort;
    }

    public MoneroDaemonConfig SetP2pUseIpv6(bool? p2pUseIpv6)
    {
        this._p2PUseIpv6 = p2pUseIpv6;
        return this;
    }

    public bool? GetP2pUseIpv6()
    {
        return _p2PUseIpv6;
    }

    public MoneroDaemonConfig SetP2pBindIpv6Address(string? p2pBindIpv6Address)
    {
        this._p2PBindIpv6Address = p2pBindIpv6Address;
        return this;
    }

    public string? GetP2pBindIpv6Address()
    {
        return _p2PBindIpv6Address;
    }

    public MoneroDaemonConfig SetP2pBindPortIpv6(int? p2pBindPortIpv6)
    {
        this._p2PBindPortIpv6 = p2pBindPortIpv6;
        return this;
    }

    public int? GetP2pBindPortIpv6()
    {
        return _p2PBindPortIpv6;
    }

    public MoneroDaemonConfig SetP2pIgnoreIpv4(bool? p2pIgnoreIpv4)
    {
        this._p2PIgnoreIpv4 = p2pIgnoreIpv4;
        return this;
    }

    public bool? GetP2pIgnoreIpv4()
    {
        return _p2PIgnoreIpv4;
    }

    public MoneroDaemonConfig SetNoIgd(bool? noIgd)
    {
        this._noIgd = noIgd;
        return this;
    }

    public bool? GetNoIgd()
    {
        return _noIgd;
    }

    public MoneroDaemonConfig SetIgd(string? igd)
    {
        this._igd = igd;
        return this;
    }

    public string? GetIgd()
    {
        return _igd;
    }

    public MoneroDaemonConfig SetHideMyPort(bool? hideMyPort)
    {
        this._hideMyPort = hideMyPort;
        return this;
    }

    public bool? GetHideMyPort()
    {
        return _hideMyPort;
    }

    public MoneroDaemonConfig SetSeedNode(string? seedNode)
    {
        this._seedNode = seedNode;
        return this;
    }

    public string? GetSeedNode()
    {
        return _seedNode;
    }

    public MoneroDaemonConfig SetPeers(List<string> peers)
    {
        this._peers = peers;
        return this;
    }

    public List<string> GetPeers()
    {
        return _peers;
    }

    public MoneroDaemonConfig AddPeer(string peer)
    {
        _peers.Add(peer);
        return this;
    }

    public MoneroDaemonConfig RemovePeer(string peer)
    {
        _peers.Remove(peer);
        return this;
    }

    public MoneroDaemonConfig RemoveAllPeers()
    {
        _peers.Clear();
        return this;
    }

    public MoneroDaemonConfig SetPriorityNodes(List<string> priorityNodes)
    {
        this._priorityNodes = priorityNodes;
        return this;
    }

    public List<string> GetPriorityNodes()
    {
        return _priorityNodes;
    }

    public MoneroDaemonConfig AddPriorityNode(string priorityNode)
    {
        _priorityNodes.Add(priorityNode);
        return this;
    }

    public MoneroDaemonConfig RemovePriorityNode(string priorityNode)
    {
        _priorityNodes.Remove(priorityNode);
        return this;
    }

    public MoneroDaemonConfig RemoveAllPriorityNodes()
    {
        _priorityNodes.Clear();
        return this;
    }

    public MoneroDaemonConfig SetExclusiveNodes(List<string> exclusiveNodes)
    {
        this._exclusiveNodes = exclusiveNodes;
        return this;
    }

    public List<string> GetExclusiveNodes()
    {
        return _exclusiveNodes;
    }

    public MoneroDaemonConfig SetOutPeers(List<string> outPeers)
    {
        this._outPeers = outPeers;
        return this;
    }

    public MoneroDaemonConfig AddOutPeer(string outPeer)
    {
        _outPeers.Add(outPeer);
        return this;
    }

    public MoneroDaemonConfig RemoveOutPeer(string outPeer)
    {
        _outPeers.Remove(outPeer);
        return this;
    }

    public MoneroDaemonConfig RemoveAllOutPeers()
    {
        _outPeers.Clear();
        return this;
    }

    public List<string> GetOutPeers()
    {
        return _outPeers;
    }

    public MoneroDaemonConfig SetInPeers(List<string> inPeers)
    {
        this._inPeers = inPeers;
        return this;
    }

    public MoneroDaemonConfig AddInPeer(string inPeer)
    {
        _inPeers.Add(inPeer);
        return this;
    }

    public MoneroDaemonConfig RemoveInPeer(string inPeer)
    {
        _inPeers.Remove(inPeer);
        return this;
    }

    public MoneroDaemonConfig RemoveAllInPeers()
    {
        _inPeers.Clear();
        return this;
    }

    public List<string> GetInPeers()
    {
        return _inPeers;
    }

    public MoneroDaemonConfig SetLimitRateUp(int? limitRateUp)
    {
        this._limitRateUp = limitRateUp;
        return this;
    }

    public int? GetLimitRateUp()
    {
        return _limitRateUp;
    }

    public MoneroDaemonConfig SetLimitRateDown(int? limitRateDown)
    {
        this._limitRateDown = limitRateDown;
        return this;
    }

    public int? GetLimitRateDown()
    {
        return _limitRateDown;
    }

    public MoneroDaemonConfig SetLimitRate(int? limitRate)
    {
        this._limitRate = limitRate;
        return this;
    }

    public int? GetLimitRate()
    {
        return _limitRate;
    }

    public MoneroDaemonConfig SetOffline(bool? offline)
    {
        this._offline = offline;
        return this;
    }

    public bool? GetOffline()
    {
        return _offline;
    }

    public MoneroDaemonConfig SetAllowLocalIp(bool? allowLocalIp)
    {
        this._allowLocalIp = allowLocalIp;
        return this;
    }

    public bool? GetAllowLocalIp()
    {
        return _allowLocalIp;
    }

    public MoneroDaemonConfig SetMaxConnectionsPerIp(int? maxConnectionsPerIp)
    {
        this._maxConnectionsPerIp = maxConnectionsPerIp;
        return this;
    }

    public int? GetMaxConnectionsPerIp()
    {
        return _maxConnectionsPerIp;
    }

    public MoneroDaemonConfig SetTorTxProxy(string? torTxProxy)
    {
        this._torTxProxy = torTxProxy;
        return this;
    }

    public string? GetTorTxProxy()
    {
        return _torTxProxy;
    }

    public MoneroDaemonConfig SetTorAnonymousInbound(string? torAnonymousInbound)
    {
        this._torAnonymousInbound = torAnonymousInbound;
        return this;
    }

    public string? GetTorAnonymousInbound()
    {
        return _torAnonymousInbound;
    }

    public MoneroDaemonConfig SetI2pTxProxy(string? i2pTxProxy)
    {
        this._i2PTxProxy = i2pTxProxy;
        return this;
    }

    public string? GetI2pTxProxy()
    {
        return _i2PTxProxy;
    }

    public MoneroDaemonConfig SetI2pAnonymousInbound(string? i2pAnonymousInbound)
    {
        this._i2PAnonymousInbound = i2pAnonymousInbound;
        return this;
    }

    public string? GetI2pAnonymousInbound()
    {
        return _i2PAnonymousInbound;
    }

    public MoneroDaemonConfig SetPadTransactions(bool? padTransactions)
    {
        this._padTransactions = padTransactions;
        return this;
    }

    public bool? GetPadTransactions()
    {
        return _padTransactions;
    }

    public MoneroDaemonConfig SetProxy(string? proxy)
    {
        this._proxy = proxy;
        return this;
    }

    public string? GetProxy()
    {
        return _proxy;
    }

    public MoneroDaemonConfig SetPublicNode(bool? publicNode)
    {
        this._publicNode = publicNode;
        return this;
    }

    public bool? GetPublicNode()
    {
        return _publicNode;
    }

    public MoneroDaemonConfig SetRpcBindIp(string? rpcBindIp)
    {
        this._rpcBindIp = rpcBindIp;
        return this;
    }

    public string? GetRpcBindIp()
    {
        return _rpcBindIp;
    }

    public MoneroDaemonConfig SetRpcBindPort(int? rpcBindPort)
    {
        this._rpcBindPort = rpcBindPort;
        return this;
    }

    public int? GetRpcBindPort()
    {
        return _rpcBindPort;
    }

    public MoneroDaemonConfig SetRpcBindIpv6Address(string? rpcBindIpv6Address)
    {
        this._rpcBindIpv6Address = rpcBindIpv6Address;
        return this;
    }

    public string? GetRpcBindIpv6Address()
    {
        return _rpcBindIpv6Address;
    }

    public MoneroDaemonConfig SetRpcUseIpv6(bool? rpcUseIpv6)
    {
        this._rpcUseIpv6 = rpcUseIpv6;
        return this;
    }

    public bool? GetRpcUseIpv6()
    {
        return _rpcUseIpv6;
    }

    public MoneroDaemonConfig SetRpcIgnoreIpv4(bool? rpcIgnoreIpv4)
    {
        this._rpcIgnoreIpv4 = rpcIgnoreIpv4;
        return this;
    }

    public bool? GetRpcIgnoreIpv4()
    {
        return _rpcIgnoreIpv4;
    }

    public MoneroDaemonConfig SetRpcRestrictedBindIp(string? rpcRestrictedBindIp)
    {
        this._rpcRestrictedBindIp = rpcRestrictedBindIp;
        return this;
    }

    public string? GetRpcRestrictedBindIp()
    {
        return _rpcRestrictedBindIp;
    }

    public MoneroDaemonConfig SetRpcRestrictedBindPort(int? rpcRestrictedBindPort)
    {
        this._rpcRestrictedBindPort = rpcRestrictedBindPort;
        return this;
    }

    public int? GetRpcRestrictedBindPort()
    {
        return _rpcRestrictedBindPort;
    }

    public MoneroDaemonConfig SetRpcRestrictedBindIpv6Address(string? rpcRestrictedBindIpv6Address)
    {
        this._rpcRestrictedBindIpv6Address = rpcRestrictedBindIpv6Address;
        return this;
    }

    public string? GetRpcRestrictedBindIpv6Address()
    {
        return _rpcRestrictedBindIpv6Address;
    }

    public MoneroDaemonConfig SetRpcMaxConnections(int? rpcMaxConnections)
    {
        this._rpcMaxConnections = rpcMaxConnections;
        return this;
    }

    public int? GetRpcMaxConnections()
    {
        return _rpcMaxConnections;
    }

    public MoneroDaemonConfig SetRpcMaxConnectionsPerPublicIp(int? rpcMaxConnectionsPerPublicIp)
    {
        this._rpcMaxConnectionsPerPublicIp = rpcMaxConnectionsPerPublicIp;
        return this;
    }

    public int? GetRpcMaxConnectionsPerPublicIp()
    {
        return _rpcMaxConnectionsPerPublicIp;
    }

    public MoneroDaemonConfig SetRpcMaxConnectionsPerPrivateIp(int? rpcMaxConnectionsPerPrivateIp)
    {
        _rpcMacConnectionsPerPrivateIp = rpcMaxConnectionsPerPrivateIp;
        return this;
    }

    public int? GetRpcMaxConnectionsPerPrivateIp()
    {
        return _rpcMacConnectionsPerPrivateIp;
    }

    public MoneroDaemonConfig SetRpcMaxResponseSoftLimit(int? rpcMaxResponseSoftLimit)
    {
        this._rpcMaxResponseSoftLimit = rpcMaxResponseSoftLimit;
        return this;
    }

    public int? GetRpcMaxResponseSoftLimit()
    {
        return _rpcMaxResponseSoftLimit;
    }

    public MoneroDaemonConfig SetRpcSsl(string? rpcSsl)
    {
        this._rpcSsl = rpcSsl;
        return this;
    }

    public string? GetRpcSsl()
    {
        return _rpcSsl;
    }

    public MoneroDaemonConfig SetRpcSslPrivateKey(string? rpcSslPrivateKey)
    {
        this._rpcSslPrivateKey = rpcSslPrivateKey;
        return this;
    }

    public string? GetRpcSslPrivateKey()
    {
        return _rpcSslPrivateKey;
    }

    public MoneroDaemonConfig SetRpcSslCertificate(string? rpcSslCertificate)
    {
        this._rpcSslCertificate = rpcSslCertificate;
        return this;
    }

    public string? GetRpcSslCertificate()
    {
        return _rpcSslCertificate;
    }

    public MoneroDaemonConfig SetRpcSslAllowedFingerprints(List<string> rpcSslAllowedFingerprints)
    {
        this._rpcSslAllowedFingerprints = rpcSslAllowedFingerprints;
        return this;
    }

    public List<string> GetRpcSslAllowedFingerprints()
    {
        return _rpcSslAllowedFingerprints;
    }

    public MoneroDaemonConfig SetRpcSslAllowAnyCert(bool? rpcSslAllowAnyCert)
    {
        this._rpcSslAllowAnyCert = rpcSslAllowAnyCert;
        return this;
    }

    public bool? GetRpcSslAllowAnyCert()
    {
        return _rpcSslAllowAnyCert;
    }

    public MoneroDaemonConfig SetRpcSslCaCertificates(string? rpcSslCaCertificates)
    {
        this._rpcSslCaCertificates = rpcSslCaCertificates;
        return this;
    }

    public string? GetRpcSslCaCertificates()
    {
        return _rpcSslCaCertificates;
    }

    public MoneroDaemonConfig SetRpcSslAllowChained(bool? rpcSslAllowChained)
    {
        this._rpcSslAllowChained = rpcSslAllowChained;
        return this;
    }

    public bool? GetRpcSslAllowChained()
    {
        return _rpcSslAllowChained;
    }

    public MoneroDaemonConfig SetRpcLogin(string? rpcLogin)
    {
        this._rpcLogin = rpcLogin;
        return this;
    }

    public string? GetRpcLogin()
    {
        return _rpcLogin;
    }

    public MoneroDaemonConfig SetRpcAccessControlOrigins(string? rpcAccessControlOrigins)
    {
        this._rpcAccessControlOrigins = rpcAccessControlOrigins;
        return this;
    }

    public string? GetRpcAccessControlOrigins()
    {
        return _rpcAccessControlOrigins;
    }

    public MoneroDaemonConfig SetDisableRpcBan(bool? disableRpcBan)
    {
        this._disableRpcBan = disableRpcBan;
        return this;
    }

    public bool? GetDisableRpcBan()
    {
        return _disableRpcBan;
    }

    public MoneroDaemonConfig SetZmqRpcBindIp(string? zmqRpcBindIp)
    {
        this._zmqRpcBindIp = zmqRpcBindIp;
        return this;
    }

    public string? GetZmqRpcBindIp()
    {
        return _zmqRpcBindIp;
    }

    public MoneroDaemonConfig SetZmqRpcBindPort(int? zmqRpcBindPort)
    {
        this._zmqRpcBindPort = zmqRpcBindPort;
        return this;
    }

    public int? GetZmqRpcBindPort()
    {
        return _zmqRpcBindPort;
    }

    public MoneroDaemonConfig SetZmqPub(string? zmqPub)
    {
        this._zmqPub = zmqPub;
        return this;
    }

    public string? GetZmqPub()
    {
        return _zmqPub;
    }

    public MoneroDaemonConfig SetNoZmq(bool? noZmq)
    {
        this._noZmq = noZmq;
        return this;
    }

    public bool? GetNoZmq()
    {
        return _noZmq;
    }

    public MoneroDaemonConfig SetConfirmExternalBind(bool? confirmExternalBind)
    {
        this._confirmExternalBind = confirmExternalBind;
        return this;
    }

    public bool? GetConfirmExternalBind()
    {
        return _confirmExternalBind;
    }

    public MoneroDaemonConfig SetRestrictedRpc(bool? restrictedRpc)
    {
        this._restrictedRpc = restrictedRpc;
        return this;
    }

    public bool? GetRestrictedRpc()
    {
        return _restrictedRpc;
    }

    public MoneroDaemonConfig SetPruneBlockchain(bool? pruneBlockchain)
    {
        this._pruneBlockchain = pruneBlockchain;
        return this;
    }

    public bool? GetPruneBlockchain()
    {
        return _pruneBlockchain;
    }

    public MoneroDaemonConfig SetSyncPrunedBlocks(bool? syncPrunedBlocks)
    {
        this._syncPrunedBlocks = syncPrunedBlocks;
        return this;
    }

    public bool? GetSyncPrunedBlocks()
    {
        return _syncPrunedBlocks;
    }

    public MoneroDaemonConfig SetDbSyncMode(string? dbSyncMode)
    {
        this._dbSyncMode = dbSyncMode;
        return this;
    }

    public string? GetDbSyncMode()
    {
        return _dbSyncMode;
    }

    public MoneroDaemonConfig SetMaxConcurrency(int? maxConcurrency)
    {
        this._maxConcurrency = maxConcurrency;
        return this;
    }

    public int? GetMaxConcurrency()
    {
        return _maxConcurrency;
    }

    public MoneroDaemonConfig SetPrepBlocksThreads(int? prepBlocksThreads)
    {
        this._prepBlocksThreads = prepBlocksThreads;
        return this;
    }

    public int? GetPrepBlocksThreads()
    {
        return _prepBlocksThreads;
    }

    public MoneroDaemonConfig SetFastBlockSync(bool? fastBlockSync)
    {
        this._fastBlockSync = fastBlockSync;
        return this;
    }

    public bool? GetFastBlockSync()
    {
        return _fastBlockSync;
    }

    public MoneroDaemonConfig SetBlockSyncSize(int? blockSyncSize)
    {
        this._blockSyncSize = blockSyncSize;
        return this;
    }

    public int? GetBlockSyncSize()
    {
        return _blockSyncSize;
    }

    public MoneroDaemonConfig SetBootstrapDaemonAddress(string? bootstrapDaemonAddress)
    {
        this._bootstrapDaemonAddress = bootstrapDaemonAddress;
        return this;
    }

    public string? GetBootstrapDaemonAddress()
    {
        return _bootstrapDaemonAddress;
    }

    public MoneroDaemonConfig SetBootstrapDaemonLogin(string? booststrapDaemonLogin)
    {
        this._booststrapDaemonLogin = booststrapDaemonLogin;
        return this;
    }

    public string? GetBootstrapDaemonLogin()
    {
        return _booststrapDaemonLogin;
    }

    public MoneroDaemonConfig SetNoSync(bool? noSync)
    {
        this._noSync = noSync;
        return this;
    }

    public bool? GetNoSync()
    {
        return _noSync;
    }

    public List<string> ToCommandList()
    {
        List<string> cmd = [];

        if (!string.IsNullOrEmpty(_path))
        {
            cmd.Add(_path);
        }

        MoneroNetworkType moneroNetworkType = GetNetworkType();

        if (moneroNetworkType == MoneroNetworkType.Testnet)
        {
            cmd.Add("--testnet");
        }
        else if (moneroNetworkType == MoneroNetworkType.Stagenet)
        {
            cmd.Add("--stagenet");
        }

        if (!string.IsNullOrEmpty(_logFile))
        {
            cmd.Add("--log-file");
            cmd.Add(_logFile);
        }

        if (_logLevel != null)
        {
            cmd.Add("--log-level");
            cmd.Add(((int)_logLevel).ToString());
        }

        if (_maxLogFileSize != null)
        {
            cmd.Add("--max-log-file-size");
            cmd.Add(((int)_maxLogFileSize).ToString());
        }

        if (_maxLogFiles != null)
        {
            cmd.Add("--max-log-files");
            cmd.Add(((int)_maxLogFiles).ToString());
        }

        if (!string.IsNullOrEmpty(_dataDir))
        {
            cmd.Add("--data-dir");
            cmd.Add(_dataDir);
        }

        if (!string.IsNullOrEmpty(_pidFile))
        {
            cmd.Add("--pid-file");
            cmd.Add(_pidFile);
        }

        if (_detach == true)
        {
            cmd.Add("--detach");
        }

        if (_nonInteractive == true)
        {
            cmd.Add("--non-interactive");
        }

        if (_maxTxPoolWeight != null)
        {
            cmd.Add("--max-txpool-weight");
            cmd.Add(((int)_maxTxPoolWeight).ToString());
        }

        if (_enforceDnsCheckpointing == true)
        {
            cmd.Add("--enforce-dns-checkpointing");
        }

        if (_disableDnsCheckpoints == true)
        {
            cmd.Add("--disable-dns-checkpoints");
        }

        if (!string.IsNullOrEmpty(_banList))
        {
            cmd.Add("--ban-list");
            cmd.Add(_banList);
        }

        if (_enableDnsBlocklist == true)
        {
            cmd.Add("--enable-dns-blocklist");
        }

        if (!string.IsNullOrEmpty(_p2PBindIp))
        {
            cmd.Add("--p2p-bind-ip");
            cmd.Add(_p2PBindIp);
        }

        if (_p2PBindPort != null && _p2PBindPort >= 0)
        {
            cmd.Add("--p2p-bind-port");
            cmd.Add(((int)_p2PBindPort).ToString());
        }

        if (_p2PExternalPort != null && _p2PExternalPort >= 0)
        {
            cmd.Add("--p2p-external-port");
            cmd.Add(((int)_p2PExternalPort).ToString());
        }

        if (_p2PUseIpv6 == true)
        {
            cmd.Add("--p2p-use-ipv6");
        }

        if (!string.IsNullOrEmpty(_p2PBindIpv6Address))
        {
            cmd.Add("--p2p-bind-ipv6-address");
            cmd.Add(_p2PBindIpv6Address);
        }

        if (_p2PBindPortIpv6 != null && _p2PBindPortIpv6 >= 0)
        {
            cmd.Add("--p2p-bind-port-ipv6");
            cmd.Add(((int)_p2PBindPortIpv6).ToString());
        }

        if (_p2PIgnoreIpv4 == true)
        {
            cmd.Add("--p2p-ignore-ipv4");
        }

        if (_noIgd == true)
        {
            cmd.Add("--no-igd");
        }

        if (!string.IsNullOrEmpty(_igd))
        {
            cmd.Add("--igd");
            cmd.Add(_igd);
        }

        if (_hideMyPort == true)
        {
            cmd.Add("--hide-my-port");
        }

        if (!string.IsNullOrEmpty(_seedNode))
        {
            cmd.Add("--seed-node");
            cmd.Add(_seedNode);
        }

        if (_peers.Count > 0)
        {
            foreach (string peer in _peers)
            {
                cmd.Add("--peer");
                cmd.Add(peer);
            }
        }

        if (_priorityNodes.Count > 0)
        {
            foreach (string priorityNode in _priorityNodes)
            {
                cmd.Add("--priority-node");
                cmd.Add(priorityNode);
            }
        }

        if (_exclusiveNodes.Count > 0)
        {
            foreach (string exclusiveNode in _exclusiveNodes)
            {
                cmd.Add("--exclusive-node");
                cmd.Add(exclusiveNode);
            }
        }

        if (_outPeers.Count > 0)
        {
            foreach (string outPeer in _outPeers)
            {
                cmd.Add("--out-peers");
                cmd.Add(outPeer);
            }
        }

        if (_inPeers.Count > 0)
        {
            foreach (string inPeer in _inPeers)
            {
                cmd.Add("--in-peers");
                cmd.Add(inPeer);
            }
        }

        if (_limitRateUp != null && _limitRateUp >= 0)
        {
            cmd.Add("--limit-rate-up");
            cmd.Add(((int)_limitRateUp).ToString());
        }

        if (_limitRateDown != null && _limitRateDown >= 0)
        {
            cmd.Add("--limit-rate-down");
            cmd.Add(((int)_limitRateDown).ToString());
        }

        if (_limitRate != null && _limitRate >= 0)
        {
            cmd.Add("--limit-rate");
            cmd.Add(((int)_limitRate).ToString());
        }

        if (_offline == true)
        {
            cmd.Add("--offline");
        }

        if (_allowLocalIp == true)
        {
            cmd.Add("--allow-local-ip");
        }

        if (_maxConnectionsPerIp != null && _maxConnectionsPerIp >= 0)
        {
            cmd.Add("--max-connections-per-ip");
            cmd.Add(((int)_maxConnectionsPerIp).ToString());
        }

        if (!string.IsNullOrEmpty(_torTxProxy))
        {
            cmd.Add("--tx-proxy");
            cmd.Add(_torTxProxy);
        }

        if (!string.IsNullOrEmpty(_torAnonymousInbound))
        {
            cmd.Add("--anonymous-inbound");
            cmd.Add(_torAnonymousInbound);
        }

        if (!string.IsNullOrEmpty(_i2PTxProxy))
        {
            cmd.Add("--tx-proxy");
            cmd.Add(_i2PTxProxy);
        }

        if (!string.IsNullOrEmpty(_i2PAnonymousInbound))
        {
            cmd.Add("--anonymous-inbound");
            cmd.Add(_i2PAnonymousInbound);
        }

        if (_padTransactions == true)
        {
            cmd.Add("--pad-transactions");
        }

        if (!string.IsNullOrEmpty(_proxy))
        {
            cmd.Add("--proxy");
            cmd.Add(_proxy);
        }

        if (_publicNode == true)
        {
            cmd.Add("--public-node");
        }

        if (!string.IsNullOrEmpty(_rpcBindIp))
        {
            cmd.Add("--rpc-bind-ip");
            cmd.Add(_rpcBindIp);
        }

        if (_rpcBindPort != null && _rpcBindPort >= 0)
        {
            cmd.Add("--rpc-bind-port");
            cmd.Add(((int)_rpcBindPort).ToString());
        }

        if (!string.IsNullOrEmpty(_rpcBindIpv6Address))
        {
            cmd.Add("--rpc-bind-ipv6-address");
            cmd.Add(_rpcBindIpv6Address);
        }

        if (_rpcUseIpv6 == true)
        {
            cmd.Add("--rpc-use-ipv6");
        }

        if (_rpcIgnoreIpv4 == true)
        {
            cmd.Add("--rpc-ignore-ipv4");
        }

        if (!string.IsNullOrEmpty(_rpcRestrictedBindIp))
        {
            cmd.Add("--rpc-restricted-bind-ip");
            cmd.Add(_rpcRestrictedBindIp);
        }

        if (_rpcRestrictedBindPort != null && _rpcRestrictedBindPort >= 0)
        {
            cmd.Add("--rpc-restricted-bind-port");
            cmd.Add(((int)_rpcRestrictedBindPort).ToString());
        }

        if (!string.IsNullOrEmpty(_rpcRestrictedBindIpv6Address))
        {
            cmd.Add("--rpc-restricted-bind-ipv6-address");
            cmd.Add(_rpcRestrictedBindIpv6Address);
        }

        if (_rpcMaxConnections != null && _rpcMaxConnections >= 0)
        {
            cmd.Add("--rpc-max-connections");
            cmd.Add(((int)_rpcMaxConnections).ToString());
        }

        if (_rpcMaxConnectionsPerPublicIp != null && _rpcMaxConnectionsPerPublicIp >= 0)
        {
            cmd.Add("--rpc-max-connections-per-public-ip");
            cmd.Add(((int)_rpcMaxConnectionsPerPublicIp).ToString());
        }

        if (_rpcMacConnectionsPerPrivateIp != null && _rpcMacConnectionsPerPrivateIp >= 0)
        {
            cmd.Add("--rpc-max-connections-per-private-ip");
            cmd.Add(((int)_rpcMacConnectionsPerPrivateIp).ToString());
        }

        if (_rpcMaxResponseSoftLimit != null && _rpcMaxResponseSoftLimit >= 0)
        {
            cmd.Add("--rpc-max-response-soft-limit");
            cmd.Add(((int)_rpcMaxResponseSoftLimit).ToString());
        }

        if (!string.IsNullOrEmpty(_rpcSsl))
        {
            cmd.Add("--rpc-ssl");
            cmd.Add(_rpcSsl);
        }

        if (!string.IsNullOrEmpty(_rpcSslPrivateKey))
        {
            cmd.Add("--rpc-ssl-private-key");
            cmd.Add(_rpcSslPrivateKey);
        }

        if (!string.IsNullOrEmpty(_rpcSslCertificate))
        {
            cmd.Add("--rpc-ssl-certificate");
            cmd.Add(_rpcSslCertificate);
        }

        if (_rpcSslAllowedFingerprints.Count > 0)
        {
            foreach (string fingerprint in _rpcSslAllowedFingerprints)
            {
                cmd.Add("--rpc-ssl-allowed-fingerprint");
                cmd.Add(fingerprint);
            }
        }

        if (_rpcSslAllowAnyCert == true)
        {
            cmd.Add("--rpc-ssl-allow-any-cert");
        }

        if (!string.IsNullOrEmpty(_rpcSslCaCertificates))
        {
            cmd.Add("--rpc-ssl-ca-certificates");
            cmd.Add(_rpcSslCaCertificates);
        }

        if (_rpcSslAllowChained == true)
        {
            cmd.Add("--rpc-ssl-allow-chained");
        }

        if (!string.IsNullOrEmpty(_rpcLogin))
        {
            cmd.Add("--rpc-login");
            cmd.Add(_rpcLogin);
        }

        if (!string.IsNullOrEmpty(_rpcAccessControlOrigins))
        {
            cmd.Add("--rpc-access-control-origins");
            cmd.Add(_rpcAccessControlOrigins);
        }

        if (_disableRpcBan == true)
        {
            cmd.Add("--disable-rpc-ban");
        }

        if (!string.IsNullOrEmpty(_zmqRpcBindIp))
        {
            cmd.Add("--zmq-rpc-bind-ip");
            cmd.Add(_zmqRpcBindIp);
        }

        if (_zmqRpcBindPort != null && _zmqRpcBindPort >= 0)
        {
            cmd.Add("--zmq-rpc-bind-port");
            cmd.Add(((int)_zmqRpcBindPort).ToString());
        }

        if (!string.IsNullOrEmpty(_zmqPub))
        {
            cmd.Add("--zmq-pub");
            cmd.Add(_zmqPub);
        }

        if (_noZmq == true)
        {
            cmd.Add("--no-zmq");
        }

        if (_confirmExternalBind == true)
        {
            cmd.Add("--confirm-external-bind");
        }

        if (_restrictedRpc == true)
        {
            cmd.Add("--restricted-rpc");
        }

        if (_pruneBlockchain == true)
        {
            cmd.Add("--prune-blockchain");
        }

        if (_syncPrunedBlocks == true)
        {
            cmd.Add("--sync-pruned-blocks");
        }

        if (!string.IsNullOrEmpty(_dbSyncMode))
        {
            cmd.Add("--db-sync-mode");
            cmd.Add(_dbSyncMode);
        }

        if (_maxConcurrency != null && _maxConcurrency >= 0)
        {
            cmd.Add("--max-concurrency");
            cmd.Add(((int)_maxConcurrency).ToString());
        }

        if (_prepBlocksThreads != null && _prepBlocksThreads >= 0)
        {
            cmd.Add("--prep-blocks-threads");
            cmd.Add(((int)_prepBlocksThreads).ToString());
        }

        if (_fastBlockSync == true)
        {
            cmd.Add("--fast-block-sync");
        }

        if (_blockSyncSize != null && _blockSyncSize >= 0)
        {
            cmd.Add("--block-sync-size");
            cmd.Add(((int)_blockSyncSize).ToString());
        }

        if (!string.IsNullOrEmpty(_bootstrapDaemonAddress))
        {
            cmd.Add("--bootstrap-daemon-address");
            cmd.Add(_bootstrapDaemonAddress);
        }

        if (!string.IsNullOrEmpty(_booststrapDaemonLogin))
        {
            cmd.Add("--bootstrap-daemon-login");
            cmd.Add(_booststrapDaemonLogin);
        }

        if (_noSync == true)
        {
            cmd.Add("--no-sync");
        }

        return cmd;
    }
}