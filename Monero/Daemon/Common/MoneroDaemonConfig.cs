using Monero.Common;

namespace Monero.Daemon.Common;

public class MoneroDaemonConfig
{
    private readonly string? path;
    private bool? allowLocalIp;
    private string? banList;
    private int? blockSyncSize;
    private string? booststrapDaemonLogin;
    private string? bootstrapDaemonAddress;
    private bool? confirmExternalBind;

    // Server
    private string? dataDir;
    private string? dbSyncMode;
    private bool? detach;
    private bool? disableDnsCheckpoints;
    private bool? disableRpcBan;

    private bool? enableDnsBlocklist;
    private bool? enforceDnsCheckpointing;
    private List<string> exclusiveNodes = [];
    private bool? fastBlockSync;
    private bool? hideMyPort;
    private string? i2pAnonymousInbound;
    private string? i2pTxProxy;
    private string? igd;
    private List<string> inPeers = [];
    private int? limitRate;
    private int? limitRateDown;
    private int? limitRateUp;

    // Log
    private string? logFile;
    private int? logLevel;
    private int? maxConcurrency;

    private int? maxConnectionsPerIp;

    private int? maxLogFiles;
    private int? maxLogFileSize;
    private ulong? maxTxPoolWeight;

    // Network type
    private MoneroNetworkType networkType = MoneroNetworkType.MAINNET;
    private bool? noIgd;
    private bool? nonInteractive;
    private bool? noSync;
    private bool? noZmq;
    private bool? offline;
    private List<string> outPeers = [];

    // P2P Netwrork
    private string? p2pBindIp;
    private string? p2pBindIpv6Address;
    private int? p2pBindPort;
    private int? p2pBindPortIpv6;
    private int? p2pExternalPort;
    private bool? p2pIgnoreIpv4;
    private bool? p2pUseIpv6;
    private bool? padTransactions;
    private List<string> peers = [];
    private string? pidFile;
    private int? prepBlocksThreads;
    private List<string> priorityNodes = [];

    private string? proxy;

    // Performance
    private bool? pruneBlockchain;

    // Node RPC API
    private bool? publicNode;

    private bool? restrictedRpc;
    private string? rpcAccessControlOrigins;
    private string? rpcBindIp;
    private string? rpcBindIpv6Address;
    private int? rpcBindPort;
    private bool? rpcIgnoreIpv4;
    private string? rpcLogin;
    private int? rpcMacConnectionsPerPrivateIp;
    private int? rpcMaxConnections;
    private int? rpcMaxConnectionsPerPublicIp;
    private int? rpcMaxResponseSoftLimit;
    private string? rpcRestrictedBindIp;
    private string? rpcRestrictedBindIpv6Address;
    private int? rpcRestrictedBindPort;
    private string? rpcSsl;
    private bool? rpcSslAllowAnyCert;
    private bool? rpcSslAllowChained;
    private List<string> rpcSslAllowedFingerprints = [];
    private string? rpcSslCaCertificates;
    private string? rpcSslCertificate;
    private string? rpcSslPrivateKey;
    private bool? rpcUseIpv6;
    private string? seedNode;
    private bool? syncPrunedBlocks;
    private string? torAnonymousInbound;

    // Tor/I2P and proxies
    private string? torTxProxy;
    private string? zmqPub;
    private string? zmqRpcBindIp;
    private int? zmqRpcBindPort;

    public MoneroDaemonConfig SetNetworkType(MoneroNetworkType networkType)
    {
        this.networkType = networkType;
        return this;
    }

    public MoneroNetworkType GetNetworkType()
    {
        return networkType;
    }

    public MoneroDaemonConfig SetLogFile(string? logFile)
    {
        this.logFile = logFile;
        return this;
    }

    public string? GetLogFile()
    {
        return logFile;
    }

    public MoneroDaemonConfig SetLogLevel(int? logLevel)
    {
        this.logLevel = logLevel;
        return this;
    }

    public int? GetLogLevel()
    {
        return logLevel;
    }

    public MoneroDaemonConfig SetMaxLogFileSize(int? maxLogFileSize)
    {
        this.maxLogFileSize = maxLogFileSize;
        return this;
    }

    public int? GetMaxLogFileSize()
    {
        return maxLogFileSize;
    }

    public MoneroDaemonConfig SetMaxLogFiles(int? maxLogFiles)
    {
        this.maxLogFiles = maxLogFiles;
        return this;
    }

    public int? GetMaxLogFiles()
    {
        return maxLogFiles;
    }

    public MoneroDaemonConfig SetDataDir(string? dataDir)
    {
        this.dataDir = dataDir;
        return this;
    }

    public string? GetDataDir()
    {
        return dataDir;
    }

    public MoneroDaemonConfig SetPidFile(string? pidFile)
    {
        this.pidFile = pidFile;
        return this;
    }

    public string? GetPidFile()
    {
        return pidFile;
    }

    public MoneroDaemonConfig SetDetach(bool? detach)
    {
        this.detach = detach;
        return this;
    }

    public bool? GetDetach()
    {
        return detach;
    }

    public MoneroDaemonConfig SetNonInteractive(bool? nonInteractive)
    {
        this.nonInteractive = nonInteractive;
        return this;
    }

    public bool? GetNonInteractive()
    {
        return nonInteractive;
    }

    public MoneroDaemonConfig SetMaxTxPoolWeight(ulong? maxTxPoolWeight)
    {
        this.maxTxPoolWeight = maxTxPoolWeight;
        return this;
    }

    public ulong? GetMaxTxPoolWeight()
    {
        return maxTxPoolWeight;
    }

    public MoneroDaemonConfig SetEnforceDnsCheckpointing(bool? enforceDnsCheckpointing)
    {
        this.enforceDnsCheckpointing = enforceDnsCheckpointing;
        return this;
    }

    public bool? GetEnforceDnsCheckpointing()
    {
        return enforceDnsCheckpointing;
    }

    public MoneroDaemonConfig SetDisableDnsCheckpoints(bool? disableDnsCheckpoints)
    {
        this.disableDnsCheckpoints = disableDnsCheckpoints;
        return this;
    }

    public bool? GetDisableDnsCheckpoints()
    {
        return disableDnsCheckpoints;
    }

    public MoneroDaemonConfig SetBanList(string? banList)
    {
        this.banList = banList;
        return this;
    }

    public string? GetBanList()
    {
        return banList;
    }

    public MoneroDaemonConfig SetEnabledDnsBlocklist(bool? enabledDnsBlocklist)
    {
        enableDnsBlocklist = enabledDnsBlocklist;
        return this;
    }

    public bool? GetEnabledDnsBlocklist()
    {
        return enableDnsBlocklist;
    }

    public MoneroDaemonConfig SetP2pBindIp(string? p2pBindIp)
    {
        this.p2pBindIp = p2pBindIp;
        return this;
    }

    public string? GetP2pBindIp()
    {
        return p2pBindIp;
    }

    public MoneroDaemonConfig SetP2pBindPort(int? p2pBindPort)
    {
        this.p2pBindPort = p2pBindPort;
        return this;
    }

    public int? GetP2pBindPort()
    {
        return p2pBindPort;
    }

    public MoneroDaemonConfig SetP2pExternalPort(int? p2pExternalPort)
    {
        this.p2pExternalPort = p2pExternalPort;
        return this;
    }

    public int? GetP2pExternalPort()
    {
        return p2pExternalPort;
    }

    public MoneroDaemonConfig SetP2pUseIpv6(bool? p2pUseIpv6)
    {
        this.p2pUseIpv6 = p2pUseIpv6;
        return this;
    }

    public bool? GetP2pUseIpv6()
    {
        return p2pUseIpv6;
    }

    public MoneroDaemonConfig SetP2pBindIpv6Address(string? p2pBindIpv6Address)
    {
        this.p2pBindIpv6Address = p2pBindIpv6Address;
        return this;
    }

    public string? GetP2pBindIpv6Address()
    {
        return p2pBindIpv6Address;
    }

    public MoneroDaemonConfig SetP2pBindPortIpv6(int? p2pBindPortIpv6)
    {
        this.p2pBindPortIpv6 = p2pBindPortIpv6;
        return this;
    }

    public int? GetP2pBindPortIpv6()
    {
        return p2pBindPortIpv6;
    }

    public MoneroDaemonConfig SetP2pIgnoreIpv4(bool? p2pIgnoreIpv4)
    {
        this.p2pIgnoreIpv4 = p2pIgnoreIpv4;
        return this;
    }

    public bool? GetP2pIgnoreIpv4()
    {
        return p2pIgnoreIpv4;
    }

    public MoneroDaemonConfig SetNoIgd(bool? noIgd)
    {
        this.noIgd = noIgd;
        return this;
    }

    public bool? GetNoIgd()
    {
        return noIgd;
    }

    public MoneroDaemonConfig SetIgd(string? igd)
    {
        this.igd = igd;
        return this;
    }

    public string? GetIgd()
    {
        return igd;
    }

    public MoneroDaemonConfig SetHideMyPort(bool? hideMyPort)
    {
        this.hideMyPort = hideMyPort;
        return this;
    }

    public bool? GetHideMyPort()
    {
        return hideMyPort;
    }

    public MoneroDaemonConfig SetSeedNode(string? seedNode)
    {
        this.seedNode = seedNode;
        return this;
    }

    public string? GetSeedNode()
    {
        return seedNode;
    }

    public MoneroDaemonConfig SetPeers(List<string>? peers)
    {
        this.peers = peers;
        return this;
    }

    public List<string> GetPeers()
    {
        return peers;
    }

    public MoneroDaemonConfig AddPeer(string peer)
    {
        peers.Add(peer);
        return this;
    }

    public MoneroDaemonConfig RemovePeer(string peer)
    {
        peers.Remove(peer);
        return this;
    }

    public MoneroDaemonConfig RemoveAllPeers()
    {
        peers.Clear();
        return this;
    }

    public MoneroDaemonConfig SetPriorityNodes(List<string>? priorityNodes)
    {
        this.priorityNodes = priorityNodes;
        return this;
    }

    public List<string> GetPriorityNodes()
    {
        return priorityNodes;
    }

    public MoneroDaemonConfig AddPriorityNode(string priorityNode)
    {
        priorityNodes.Add(priorityNode);
        return this;
    }

    public MoneroDaemonConfig RemovePriorityNode(string priorityNode)
    {
        priorityNodes.Remove(priorityNode);
        return this;
    }

    public MoneroDaemonConfig RemoveAllPriorityNodes()
    {
        priorityNodes.Clear();
        return this;
    }

    public MoneroDaemonConfig SetExclusiveNodes(List<string>? exclusiveNodes)
    {
        this.exclusiveNodes = exclusiveNodes;
        return this;
    }

    public List<string> GetExclusiveNodes()
    {
        return exclusiveNodes;
    }

    public MoneroDaemonConfig SetOutPeers(List<string> outPeers)
    {
        this.outPeers = outPeers;
        return this;
    }

    public MoneroDaemonConfig AddOutPeer(string outPeer)
    {
        outPeers.Add(outPeer);
        return this;
    }

    public MoneroDaemonConfig RemoveOutPeer(string outPeer)
    {
        outPeers.Remove(outPeer);
        return this;
    }

    public MoneroDaemonConfig RemoveAllOutPeers()
    {
        outPeers.Clear();
        return this;
    }

    public List<string> GetOutPeers()
    {
        return outPeers;
    }

    public MoneroDaemonConfig SetInPeers(List<string> inPeers)
    {
        this.inPeers = inPeers;
        return this;
    }

    public MoneroDaemonConfig AddInPeer(string inPeer)
    {
        inPeers.Add(inPeer);
        return this;
    }

    public MoneroDaemonConfig RemoveInPeer(string inPeer)
    {
        inPeers.Remove(inPeer);
        return this;
    }

    public MoneroDaemonConfig RemoveAllInPeers()
    {
        inPeers.Clear();
        return this;
    }

    public List<string> GetInPeers()
    {
        return inPeers;
    }

    public MoneroDaemonConfig SetLimitRateUp(int? limitRateUp)
    {
        this.limitRateUp = limitRateUp;
        return this;
    }

    public int? GetLimitRateUp()
    {
        return limitRateUp;
    }

    public MoneroDaemonConfig SetLimitRateDown(int? limitRateDown)
    {
        this.limitRateDown = limitRateDown;
        return this;
    }

    public int? GetLimitRateDown()
    {
        return limitRateDown;
    }

    public MoneroDaemonConfig SetLimitRate(int? limitRate)
    {
        this.limitRate = limitRate;
        return this;
    }

    public int? GetLimitRate()
    {
        return limitRate;
    }

    public MoneroDaemonConfig SetOffline(bool? offline)
    {
        this.offline = offline;
        return this;
    }

    public bool? GetOffline()
    {
        return offline;
    }

    public MoneroDaemonConfig SetAllowLocalIp(bool? allowLocalIp)
    {
        this.allowLocalIp = allowLocalIp;
        return this;
    }

    public bool? GetAllowLocalIp()
    {
        return allowLocalIp;
    }

    public MoneroDaemonConfig SetMaxConnectionsPerIp(int? maxConnectionsPerIp)
    {
        this.maxConnectionsPerIp = maxConnectionsPerIp;
        return this;
    }

    public int? GetMaxConnectionsPerIp()
    {
        return maxConnectionsPerIp;
    }

    public MoneroDaemonConfig SetTorTxProxy(string? torTxProxy)
    {
        this.torTxProxy = torTxProxy;
        return this;
    }

    public string? GetTorTxProxy()
    {
        return torTxProxy;
    }

    public MoneroDaemonConfig SetTorAnonymousInbound(string? torAnonymousInbound)
    {
        this.torAnonymousInbound = torAnonymousInbound;
        return this;
    }

    public string? GetTorAnonymousInbound()
    {
        return torAnonymousInbound;
    }

    public MoneroDaemonConfig SetI2pTxProxy(string? i2pTxProxy)
    {
        this.i2pTxProxy = i2pTxProxy;
        return this;
    }

    public string? GetI2pTxProxy()
    {
        return i2pTxProxy;
    }

    public MoneroDaemonConfig SetI2pAnonymousInbound(string? i2pAnonymousInbound)
    {
        this.i2pAnonymousInbound = i2pAnonymousInbound;
        return this;
    }

    public string? GetI2pAnonymousInbound()
    {
        return i2pAnonymousInbound;
    }

    public MoneroDaemonConfig SetPadTransactions(bool? padTransactions)
    {
        this.padTransactions = padTransactions;
        return this;
    }

    public bool? GetPadTransactions()
    {
        return padTransactions;
    }

    public MoneroDaemonConfig SetProxy(string? proxy)
    {
        this.proxy = proxy;
        return this;
    }

    public string? GetProxy()
    {
        return proxy;
    }

    public MoneroDaemonConfig SetPublicNode(bool? publicNode)
    {
        this.publicNode = publicNode;
        return this;
    }

    public bool? GetPublicNode()
    {
        return publicNode;
    }

    public MoneroDaemonConfig SetRpcBindIp(string? rpcBindIp)
    {
        this.rpcBindIp = rpcBindIp;
        return this;
    }

    public string? GetRpcBindIp()
    {
        return rpcBindIp;
    }

    public MoneroDaemonConfig SetRpcBindPort(int? rpcBindPort)
    {
        this.rpcBindPort = rpcBindPort;
        return this;
    }

    public int? GetRpcBindPort()
    {
        return rpcBindPort;
    }

    public MoneroDaemonConfig SetRpcBindIpv6Address(string? rpcBindIpv6Address)
    {
        this.rpcBindIpv6Address = rpcBindIpv6Address;
        return this;
    }

    public string? GetRpcBindIpv6Address()
    {
        return rpcBindIpv6Address;
    }

    public MoneroDaemonConfig SetRpcUseIpv6(bool? rpcUseIpv6)
    {
        this.rpcUseIpv6 = rpcUseIpv6;
        return this;
    }

    public bool? GetRpcUseIpv6()
    {
        return rpcUseIpv6;
    }

    public MoneroDaemonConfig SetRpcIgnoreIpv4(bool? rpcIgnoreIpv4)
    {
        this.rpcIgnoreIpv4 = rpcIgnoreIpv4;
        return this;
    }

    public bool? GetRpcIgnoreIpv4()
    {
        return rpcIgnoreIpv4;
    }

    public MoneroDaemonConfig SetRpcRestrictedBindIp(string? rpcRestrictedBindIp)
    {
        this.rpcRestrictedBindIp = rpcRestrictedBindIp;
        return this;
    }

    public string? GetRpcRestrictedBindIp()
    {
        return rpcRestrictedBindIp;
    }

    public MoneroDaemonConfig SetRpcRestrictedBindPort(int? rpcRestrictedBindPort)
    {
        this.rpcRestrictedBindPort = rpcRestrictedBindPort;
        return this;
    }

    public int? GetRpcRestrictedBindPort()
    {
        return rpcRestrictedBindPort;
    }

    public MoneroDaemonConfig SetRpcRestrictedBindIpv6Address(string? rpcRestrictedBindIpv6Address)
    {
        this.rpcRestrictedBindIpv6Address = rpcRestrictedBindIpv6Address;
        return this;
    }

    public string? GetRpcRestrictedBindIpv6Address()
    {
        return rpcRestrictedBindIpv6Address;
    }

    public MoneroDaemonConfig SetRpcMaxConnections(int? rpcMaxConnections)
    {
        this.rpcMaxConnections = rpcMaxConnections;
        return this;
    }

    public int? GetRpcMaxConnections()
    {
        return rpcMaxConnections;
    }

    public MoneroDaemonConfig SetRpcMaxConnectionsPerPublicIp(int? rpcMaxConnectionsPerPublicIp)
    {
        this.rpcMaxConnectionsPerPublicIp = rpcMaxConnectionsPerPublicIp;
        return this;
    }

    public int? GetRpcMaxConnectionsPerPublicIp()
    {
        return rpcMaxConnectionsPerPublicIp;
    }

    public MoneroDaemonConfig SetRpcMaxConnectionsPerPrivateIp(int? rpcMaxConnectionsPerPrivateIp)
    {
        rpcMacConnectionsPerPrivateIp = rpcMaxConnectionsPerPrivateIp;
        return this;
    }

    public int? GetRpcMaxConnectionsPerPrivateIp()
    {
        return rpcMacConnectionsPerPrivateIp;
    }

    public MoneroDaemonConfig SetRpcMaxResponseSoftLimit(int? rpcMaxResponseSoftLimit)
    {
        this.rpcMaxResponseSoftLimit = rpcMaxResponseSoftLimit;
        return this;
    }

    public int? GetRpcMaxResponseSoftLimit()
    {
        return rpcMaxResponseSoftLimit;
    }

    public MoneroDaemonConfig SetRpcSsl(string? rpcSsl)
    {
        this.rpcSsl = rpcSsl;
        return this;
    }

    public string? GetRpcSsl()
    {
        return rpcSsl;
    }

    public MoneroDaemonConfig SetRpcSslPrivateKey(string? rpcSslPrivateKey)
    {
        this.rpcSslPrivateKey = rpcSslPrivateKey;
        return this;
    }

    public string? GetRpcSslPrivateKey()
    {
        return rpcSslPrivateKey;
    }

    public MoneroDaemonConfig SetRpcSslCertificate(string? rpcSslCertificate)
    {
        this.rpcSslCertificate = rpcSslCertificate;
        return this;
    }

    public string? GetRpcSslCertificate()
    {
        return rpcSslCertificate;
    }

    public MoneroDaemonConfig SetRpcSslAllowedFingerprints(List<string> rpcSslAllowedFingerprints)
    {
        this.rpcSslAllowedFingerprints = rpcSslAllowedFingerprints;
        return this;
    }

    public List<string> GetRpcSslAllowedFingerprints()
    {
        return rpcSslAllowedFingerprints;
    }

    public MoneroDaemonConfig SetRpcSslAllowAnyCert(bool? rpcSslAllowAnyCert)
    {
        this.rpcSslAllowAnyCert = rpcSslAllowAnyCert;
        return this;
    }

    public bool? GetRpcSslAllowAnyCert()
    {
        return rpcSslAllowAnyCert;
    }

    public MoneroDaemonConfig SetRpcSslCaCertificates(string? rpcSslCaCertificates)
    {
        this.rpcSslCaCertificates = rpcSslCaCertificates;
        return this;
    }

    public string? GetRpcSslCaCertificates()
    {
        return rpcSslCaCertificates;
    }

    public MoneroDaemonConfig SetRpcSslAllowChained(bool? rpcSslAllowChained)
    {
        this.rpcSslAllowChained = rpcSslAllowChained;
        return this;
    }

    public bool? GetRpcSslAllowChained()
    {
        return rpcSslAllowChained;
    }

    public MoneroDaemonConfig SetRpcLogin(string? rpcLogin)
    {
        this.rpcLogin = rpcLogin;
        return this;
    }

    public string? GetRpcLogin()
    {
        return rpcLogin;
    }

    public MoneroDaemonConfig SetRpcAccessControlOrigins(string? rpcAccessControlOrigins)
    {
        this.rpcAccessControlOrigins = rpcAccessControlOrigins;
        return this;
    }

    public string? GetRpcAccessControlOrigins()
    {
        return rpcAccessControlOrigins;
    }

    public MoneroDaemonConfig SetDisableRpcBan(bool? disableRpcBan)
    {
        this.disableRpcBan = disableRpcBan;
        return this;
    }

    public bool? GetDisableRpcBan()
    {
        return disableRpcBan;
    }

    public MoneroDaemonConfig SetZmqRpcBindIp(string? zmqRpcBindIp)
    {
        this.zmqRpcBindIp = zmqRpcBindIp;
        return this;
    }

    public string? GetZmqRpcBindIp()
    {
        return zmqRpcBindIp;
    }

    public MoneroDaemonConfig SetZmqRpcBindPort(int? zmqRpcBindPort)
    {
        this.zmqRpcBindPort = zmqRpcBindPort;
        return this;
    }

    public int? GetZmqRpcBindPort()
    {
        return zmqRpcBindPort;
    }

    public MoneroDaemonConfig SetZmqPub(string? zmqPub)
    {
        this.zmqPub = zmqPub;
        return this;
    }

    public string? GetZmqPub()
    {
        return zmqPub;
    }

    public MoneroDaemonConfig SetNoZmq(bool? noZmq)
    {
        this.noZmq = noZmq;
        return this;
    }

    public bool? GetNoZmq()
    {
        return noZmq;
    }

    public MoneroDaemonConfig SetConfirmExternalBind(bool? confirmExternalBind)
    {
        this.confirmExternalBind = confirmExternalBind;
        return this;
    }

    public bool? GetConfirmExternalBind()
    {
        return confirmExternalBind;
    }

    public MoneroDaemonConfig SetRestrictedRpc(bool? restrictedRpc)
    {
        this.restrictedRpc = restrictedRpc;
        return this;
    }

    public bool? GetRestrictedRpc()
    {
        return restrictedRpc;
    }

    public MoneroDaemonConfig SetPruneBlockchain(bool? pruneBlockchain)
    {
        this.pruneBlockchain = pruneBlockchain;
        return this;
    }

    public bool? GetPruneBlockchain()
    {
        return pruneBlockchain;
    }

    public MoneroDaemonConfig SetSyncPrunedBlocks(bool? syncPrunedBlocks)
    {
        this.syncPrunedBlocks = syncPrunedBlocks;
        return this;
    }

    public bool? GetSyncPrunedBlocks()
    {
        return syncPrunedBlocks;
    }

    public MoneroDaemonConfig SetDbSyncMode(string? dbSyncMode)
    {
        this.dbSyncMode = dbSyncMode;
        return this;
    }

    public string? GetDbSyncMode()
    {
        return dbSyncMode;
    }

    public MoneroDaemonConfig SetMaxConcurrency(int? maxConcurrency)
    {
        this.maxConcurrency = maxConcurrency;
        return this;
    }

    public int? GetMaxConcurrency()
    {
        return maxConcurrency;
    }

    public MoneroDaemonConfig SetPrepBlocksThreads(int? prepBlocksThreads)
    {
        this.prepBlocksThreads = prepBlocksThreads;
        return this;
    }

    public int? GetPrepBlocksThreads()
    {
        return prepBlocksThreads;
    }

    public MoneroDaemonConfig SetFastBlockSync(bool? fastBlockSync)
    {
        this.fastBlockSync = fastBlockSync;
        return this;
    }

    public bool? GetFastBlockSync()
    {
        return fastBlockSync;
    }

    public MoneroDaemonConfig SetBlockSyncSize(int? blockSyncSize)
    {
        this.blockSyncSize = blockSyncSize;
        return this;
    }

    public int? GetBlockSyncSize()
    {
        return blockSyncSize;
    }

    public MoneroDaemonConfig SetBootstrapDaemonAddress(string? bootstrapDaemonAddress)
    {
        this.bootstrapDaemonAddress = bootstrapDaemonAddress;
        return this;
    }

    public string? GetBootstrapDaemonAddress()
    {
        return bootstrapDaemonAddress;
    }

    public MoneroDaemonConfig SetBootstrapDaemonLogin(string? booststrapDaemonLogin)
    {
        this.booststrapDaemonLogin = booststrapDaemonLogin;
        return this;
    }

    public string? GetBootstrapDaemonLogin()
    {
        return booststrapDaemonLogin;
    }

    public MoneroDaemonConfig SetNoSync(bool? noSync)
    {
        this.noSync = noSync;
        return this;
    }

    public bool? GetNoSync()
    {
        return noSync;
    }

    public List<string> ToCommandList()
    {
        List<string> cmd = [];

        if (!string.IsNullOrEmpty(path))
        {
            cmd.Add(path);
        }

        MoneroNetworkType networkType = GetNetworkType();

        if (networkType == MoneroNetworkType.TESTNET)
        {
            cmd.Add("--testnet");
        }
        else if (networkType == MoneroNetworkType.STAGENET)
        {
            cmd.Add("--stagenet");
        }

        if (!string.IsNullOrEmpty(logFile))
        {
            cmd.Add("--log-file");
            cmd.Add(logFile);
        }

        if (logLevel != null)
        {
            cmd.Add("--log-level");
            cmd.Add(((int)logLevel).ToString());
        }

        if (maxLogFileSize != null)
        {
            cmd.Add("--max-log-file-size");
            cmd.Add(((int)maxLogFileSize).ToString());
        }

        if (maxLogFiles != null)
        {
            cmd.Add("--max-log-files");
            cmd.Add(((int)maxLogFiles).ToString());
        }

        if (!string.IsNullOrEmpty(dataDir))
        {
            cmd.Add("--data-dir");
            cmd.Add(dataDir);
        }

        if (!string.IsNullOrEmpty(pidFile))
        {
            cmd.Add("--pid-file");
            cmd.Add(pidFile);
        }

        if (detach == true)
        {
            cmd.Add("--detach");
        }

        if (nonInteractive == true)
        {
            cmd.Add("--non-interactive");
        }

        if (maxTxPoolWeight != null)
        {
            cmd.Add("--max-txpool-weight");
            cmd.Add(((int)maxTxPoolWeight).ToString());
        }

        if (enforceDnsCheckpointing == true)
        {
            cmd.Add("--enforce-dns-checkpointing");
        }

        if (disableDnsCheckpoints == true)
        {
            cmd.Add("--disable-dns-checkpoints");
        }

        if (!string.IsNullOrEmpty(banList))
        {
            cmd.Add("--ban-list");
            cmd.Add(banList);
        }

        if (enableDnsBlocklist == true)
        {
            cmd.Add("--enable-dns-blocklist");
        }

        if (!string.IsNullOrEmpty(p2pBindIp))
        {
            cmd.Add("--p2p-bind-ip");
            cmd.Add(p2pBindIp);
        }

        if (p2pBindPort != null && p2pBindPort >= 0)
        {
            cmd.Add("--p2p-bind-port");
            cmd.Add(((int)p2pBindPort).ToString());
        }

        if (p2pExternalPort != null && p2pExternalPort >= 0)
        {
            cmd.Add("--p2p-external-port");
            cmd.Add(((int)p2pExternalPort).ToString());
        }

        if (p2pUseIpv6 == true)
        {
            cmd.Add("--p2p-use-ipv6");
        }

        if (!string.IsNullOrEmpty(p2pBindIpv6Address))
        {
            cmd.Add("--p2p-bind-ipv6-address");
            cmd.Add(p2pBindIpv6Address);
        }

        if (p2pBindPortIpv6 != null && p2pBindPortIpv6 >= 0)
        {
            cmd.Add("--p2p-bind-port-ipv6");
            cmd.Add(((int)p2pBindPortIpv6).ToString());
        }

        if (p2pIgnoreIpv4 == true)
        {
            cmd.Add("--p2p-ignore-ipv4");
        }

        if (noIgd == true)
        {
            cmd.Add("--no-igd");
        }

        if (!string.IsNullOrEmpty(igd))
        {
            cmd.Add("--igd");
            cmd.Add(igd);
        }

        if (hideMyPort == true)
        {
            cmd.Add("--hide-my-port");
        }

        if (!string.IsNullOrEmpty(seedNode))
        {
            cmd.Add("--seed-node");
            cmd.Add(seedNode);
        }

        if (peers.Count > 0)
        {
            foreach (string peer in peers)
            {
                cmd.Add("--peer");
                cmd.Add(peer);
            }
        }

        if (priorityNodes.Count > 0)
        {
            foreach (string priorityNode in priorityNodes)
            {
                cmd.Add("--priority-node");
                cmd.Add(priorityNode);
            }
        }

        if (exclusiveNodes.Count > 0)
        {
            foreach (string exclusiveNode in exclusiveNodes)
            {
                cmd.Add("--exclusive-node");
                cmd.Add(exclusiveNode);
            }
        }

        if (outPeers.Count > 0)
        {
            foreach (string outPeer in outPeers)
            {
                cmd.Add("--out-peers");
                cmd.Add(outPeer);
            }
        }

        if (inPeers.Count > 0)
        {
            foreach (string inPeer in inPeers)
            {
                cmd.Add("--in-peers");
                cmd.Add(inPeer);
            }
        }

        if (limitRateUp != null && limitRateUp >= 0)
        {
            cmd.Add("--limit-rate-up");
            cmd.Add(((int)limitRateUp).ToString());
        }

        if (limitRateDown != null && limitRateDown >= 0)
        {
            cmd.Add("--limit-rate-down");
            cmd.Add(((int)limitRateDown).ToString());
        }

        if (limitRate != null && limitRate >= 0)
        {
            cmd.Add("--limit-rate");
            cmd.Add(((int)limitRate).ToString());
        }

        if (offline == true)
        {
            cmd.Add("--offline");
        }

        if (allowLocalIp == true)
        {
            cmd.Add("--allow-local-ip");
        }

        if (maxConnectionsPerIp != null && maxConnectionsPerIp >= 0)
        {
            cmd.Add("--max-connections-per-ip");
            cmd.Add(((int)maxConnectionsPerIp).ToString());
        }

        if (!string.IsNullOrEmpty(torTxProxy))
        {
            cmd.Add("--tx-proxy");
            cmd.Add(torTxProxy);
        }

        if (!string.IsNullOrEmpty(torAnonymousInbound))
        {
            cmd.Add("--anonymous-inbound");
            cmd.Add(torAnonymousInbound);
        }

        if (!string.IsNullOrEmpty(i2pTxProxy))
        {
            cmd.Add("--tx-proxy");
            cmd.Add(i2pTxProxy);
        }

        if (!string.IsNullOrEmpty(i2pAnonymousInbound))
        {
            cmd.Add("--anonymous-inbound");
            cmd.Add(i2pAnonymousInbound);
        }

        if (padTransactions == true)
        {
            cmd.Add("--pad-transactions");
        }

        if (!string.IsNullOrEmpty(proxy))
        {
            cmd.Add("--proxy");
            cmd.Add(proxy);
        }

        if (publicNode == true)
        {
            cmd.Add("--public-node");
        }

        if (!string.IsNullOrEmpty(rpcBindIp))
        {
            cmd.Add("--rpc-bind-ip");
            cmd.Add(rpcBindIp);
        }

        if (rpcBindPort != null && rpcBindPort >= 0)
        {
            cmd.Add("--rpc-bind-port");
            cmd.Add(((int)rpcBindPort).ToString());
        }

        if (!string.IsNullOrEmpty(rpcBindIpv6Address))
        {
            cmd.Add("--rpc-bind-ipv6-address");
            cmd.Add(rpcBindIpv6Address);
        }

        if (rpcUseIpv6 == true)
        {
            cmd.Add("--rpc-use-ipv6");
        }

        if (rpcIgnoreIpv4 == true)
        {
            cmd.Add("--rpc-ignore-ipv4");
        }

        if (!string.IsNullOrEmpty(rpcRestrictedBindIp))
        {
            cmd.Add("--rpc-restricted-bind-ip");
            cmd.Add(rpcRestrictedBindIp);
        }

        if (rpcRestrictedBindPort != null && rpcRestrictedBindPort >= 0)
        {
            cmd.Add("--rpc-restricted-bind-port");
            cmd.Add(((int)rpcRestrictedBindPort).ToString());
        }

        if (!string.IsNullOrEmpty(rpcRestrictedBindIpv6Address))
        {
            cmd.Add("--rpc-restricted-bind-ipv6-address");
            cmd.Add(rpcRestrictedBindIpv6Address);
        }

        if (rpcMaxConnections != null && rpcMaxConnections >= 0)
        {
            cmd.Add("--rpc-max-connections");
            cmd.Add(((int)rpcMaxConnections).ToString());
        }

        if (rpcMaxConnectionsPerPublicIp != null && rpcMaxConnectionsPerPublicIp >= 0)
        {
            cmd.Add("--rpc-max-connections-per-public-ip");
            cmd.Add(((int)rpcMaxConnectionsPerPublicIp).ToString());
        }

        if (rpcMacConnectionsPerPrivateIp != null && rpcMacConnectionsPerPrivateIp >= 0)
        {
            cmd.Add("--rpc-max-connections-per-private-ip");
            cmd.Add(((int)rpcMacConnectionsPerPrivateIp).ToString());
        }

        if (rpcMaxResponseSoftLimit != null && rpcMaxResponseSoftLimit >= 0)
        {
            cmd.Add("--rpc-max-response-soft-limit");
            cmd.Add(((int)rpcMaxResponseSoftLimit).ToString());
        }

        if (!string.IsNullOrEmpty(rpcSsl))
        {
            cmd.Add("--rpc-ssl");
            cmd.Add(rpcSsl);
        }

        if (!string.IsNullOrEmpty(rpcSslPrivateKey))
        {
            cmd.Add("--rpc-ssl-private-key");
            cmd.Add(rpcSslPrivateKey);
        }

        if (!string.IsNullOrEmpty(rpcSslCertificate))
        {
            cmd.Add("--rpc-ssl-certificate");
            cmd.Add(rpcSslCertificate);
        }

        if (rpcSslAllowedFingerprints.Count > 0)
        {
            foreach (string fingerprint in rpcSslAllowedFingerprints)
            {
                cmd.Add("--rpc-ssl-allowed-fingerprint");
                cmd.Add(fingerprint);
            }
        }

        if (rpcSslAllowAnyCert == true)
        {
            cmd.Add("--rpc-ssl-allow-any-cert");
        }

        if (!string.IsNullOrEmpty(rpcSslCaCertificates))
        {
            cmd.Add("--rpc-ssl-ca-certificates");
            cmd.Add(rpcSslCaCertificates);
        }

        if (rpcSslAllowChained == true)
        {
            cmd.Add("--rpc-ssl-allow-chained");
        }

        if (!string.IsNullOrEmpty(rpcLogin))
        {
            cmd.Add("--rpc-login");
            cmd.Add(rpcLogin);
        }

        if (!string.IsNullOrEmpty(rpcAccessControlOrigins))
        {
            cmd.Add("--rpc-access-control-origins");
            cmd.Add(rpcAccessControlOrigins);
        }

        if (disableRpcBan == true)
        {
            cmd.Add("--disable-rpc-ban");
        }

        if (!string.IsNullOrEmpty(zmqRpcBindIp))
        {
            cmd.Add("--zmq-rpc-bind-ip");
            cmd.Add(zmqRpcBindIp);
        }

        if (zmqRpcBindPort != null && zmqRpcBindPort >= 0)
        {
            cmd.Add("--zmq-rpc-bind-port");
            cmd.Add(((int)zmqRpcBindPort).ToString());
        }

        if (!string.IsNullOrEmpty(zmqPub))
        {
            cmd.Add("--zmq-pub");
            cmd.Add(zmqPub);
        }

        if (noZmq == true)
        {
            cmd.Add("--no-zmq");
        }

        if (confirmExternalBind == true)
        {
            cmd.Add("--confirm-external-bind");
        }

        if (restrictedRpc == true)
        {
            cmd.Add("--restricted-rpc");
        }

        if (pruneBlockchain == true)
        {
            cmd.Add("--prune-blockchain");
        }

        if (syncPrunedBlocks == true)
        {
            cmd.Add("--sync-pruned-blocks");
        }

        if (!string.IsNullOrEmpty(dbSyncMode))
        {
            cmd.Add("--db-sync-mode");
            cmd.Add(dbSyncMode);
        }

        if (maxConcurrency != null && maxConcurrency >= 0)
        {
            cmd.Add("--max-concurrency");
            cmd.Add(((int)maxConcurrency).ToString());
        }

        if (prepBlocksThreads != null && prepBlocksThreads >= 0)
        {
            cmd.Add("--prep-blocks-threads");
            cmd.Add(((int)prepBlocksThreads).ToString());
        }

        if (fastBlockSync == true)
        {
            cmd.Add("--fast-block-sync");
        }

        if (blockSyncSize != null && blockSyncSize >= 0)
        {
            cmd.Add("--block-sync-size");
            cmd.Add(((int)blockSyncSize).ToString());
        }

        if (!string.IsNullOrEmpty(bootstrapDaemonAddress))
        {
            cmd.Add("--bootstrap-daemon-address");
            cmd.Add(bootstrapDaemonAddress);
        }

        if (!string.IsNullOrEmpty(booststrapDaemonLogin))
        {
            cmd.Add("--bootstrap-daemon-login");
            cmd.Add(booststrapDaemonLogin);
        }

        if (noSync == true)
        {
            cmd.Add("--no-sync");
        }

        return cmd;
    }
}