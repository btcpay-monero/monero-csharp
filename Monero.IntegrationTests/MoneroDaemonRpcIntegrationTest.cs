using Monero.Common;
using Monero.Daemon;
using Monero.Daemon.Common;
using Monero.Daemon.Rpc;
using Monero.IntegrationTests.Utils;

using Xunit;

namespace Monero.IntegrationTests;

public class MoneroDaemonRpcIntegrationTest
{
    private readonly MoneroDaemonRpc _daemon = TestUtils.GetDaemonRpc(); // daemon instance to test

    public MoneroDaemonRpcIntegrationTest()
    {
        ulong daemonHeight = _daemon.GetHeight().GetAwaiter().GetResult();
        if (daemonHeight == 1)
        {
            _daemon.WaitForNextBlockHeader().GetAwaiter().GetResult();
        }
    }

    #region Notification Tests

    // Can notify listeners when a new block is added to the chain
    [Fact]
    public async Task TestBlockListener()
    {
        try
        {
            // register a listener
            MoneroDaemonListener listener = new();
            _daemon.AddListener(listener);

            // wait for the next block notification
            MoneroBlockHeader header = await _daemon.WaitForNextBlockHeader();
            _daemon.RemoveListener(listener); // unregister listener so daemon does not keep polling
            TestBlockHeader(header, true);

            // test that listener was called with the equivalent header
            Assert.True(header.Equals(listener.GetLastBlockHeader()));
        }
        finally
        {
            // stop mining
            try { await _daemon.StopMining(); }
            catch (MoneroError)
            {
                // ignore
            }
        }
    }

    #endregion

    #region Non Relays Tests

    [Fact]
    public async Task TestGetVersion()
    {
        MoneroVersion version = await _daemon.GetVersion();
        Assert.NotNull(version.Number);
        Assert.True(version.Number > 0);
        Assert.NotNull(version.IsRelease);
    }

    // Can get the blockchain height
    [Fact]
    public async Task TestGetHeight()
    {
        ulong height = await _daemon.GetHeight();
        Assert.True(height > 0, "Height must be greater than 0");
    }

    // Can get a block hash by height
    [Fact]
    public async Task TestGetBlockIdByHeight()
    {
        MoneroBlockHeader lastHeader = await _daemon.GetLastBlockHeader();
        string hash = await _daemon.GetBlockHash((ulong)lastHeader.GetHeight()!);
        Assert.NotNull(hash);
        Assert.Equal(64, hash.Length);
    }

    // Can get a block template
    [Fact]
    public async Task TestGetBlockTemplate()
    {
        MoneroBlockTemplate template = await _daemon.GetBlockTemplate(TestUtils.Address, 2);
        TestBlockTemplate(template);
    }

    // Can get the last block's header
    [Fact]
    public async Task TestGetLastBlockHeader()
    {
        MoneroBlockHeader lastHeader = await _daemon.GetLastBlockHeader();
        TestBlockHeader(lastHeader, true);
    }

    // Can get a block header by hash
    [Fact]
    public async Task TestGetBlockHeaderByHash()
    {
        // retrieve by hash of the last block
        MoneroBlockHeader lastHeader = await _daemon.GetLastBlockHeader();
        string hash = await _daemon.GetBlockHash((ulong)lastHeader.GetHeight()!);
        MoneroBlockHeader header = await _daemon.GetBlockHeaderByHash(hash);
        TestBlockHeader(header, true);
        Assert.True(lastHeader.Equals(header));

        // retrieve by hash of previous to last block
        hash = await _daemon.GetBlockHash((ulong)lastHeader.GetHeight()! - 1);
        header = await _daemon.GetBlockHeaderByHash(hash);
        TestBlockHeader(header, true);
        Assert.True(lastHeader.GetHeight() - 1 == (ulong)header.GetHeight()!);
    }

    // Can get a block header by height
    [Fact]
    public async Task TestGetBlockHeaderByHeight()
    {
        // retrieve by height of the last block
        MoneroBlockHeader lastHeader = await _daemon.GetLastBlockHeader();
        MoneroBlockHeader header = await _daemon.GetBlockHeaderByHeight((ulong)lastHeader.GetHeight()!);
        TestBlockHeader(header, true);
        Assert.True(lastHeader.Equals(header));

        // retrieve by height of previous to last block
        header = await _daemon.GetBlockHeaderByHeight((ulong)lastHeader.GetHeight()! - 1);
        TestBlockHeader(header, true);
        Assert.True(lastHeader.GetHeight() - 1 == (ulong)header.GetHeight()!);
    }

    // Can get block headers by range
    // TODO: test start with no end, vice versa, inclusivity
    [Fact]
    public async Task TestGetBlockHeadersByRange()
    {
        // determine start and end height based on the number of blocks and how many blocks ago
        ulong numBlocks = 10;
        ulong numBlocksAgo = 10;
        ulong currentHeight = await _daemon.GetHeight();
        ulong startHeight = currentHeight - numBlocksAgo;
        ulong endHeight = currentHeight - 1;

        // fetch headers
        List<MoneroBlockHeader> headers = await
            _daemon.GetBlockHeadersByRange(startHeight, endHeight);

        // test headers
        Assert.True(numBlocks == (ulong)headers.Count);
        int j = 0;
        for (ulong i = 0; i < numBlocks; i++)
        {
            MoneroBlockHeader header = headers[j];
            Assert.True(startHeight + i == (ulong)header.GetHeight()!);
            TestBlockHeader(header, true);
            j++;
        }
    }

    // Can get a block by hash
    [Fact]
    public async Task TestGetBlockByHash()
    {
        // test config
        TestContext ctx = new() { HasHex = true, HasTxs = false, HeaderIsFull = true };

        // retrieve by hash of the last block
        MoneroBlockHeader lastHeader = await _daemon.GetLastBlockHeader();
        string hash = await _daemon.GetBlockHash((ulong)lastHeader.GetHeight()!);
        MoneroBlock block = await _daemon.GetBlockByHash(hash);
        TestBlock(block, ctx);
        Assert.True((await _daemon.GetBlockByHeight((ulong)block.GetHeight()!)).Equals(block));
        Assert.Null(block.Txs);

        // retrieve by hash of previous to last block
        hash = await _daemon.GetBlockHash((ulong)lastHeader.GetHeight()! - 1);
        block = await _daemon.GetBlockByHash(hash);
        TestBlock(block, ctx);
        Assert.True((await _daemon.GetBlockByHeight((ulong)lastHeader.GetHeight()! - 1)).Equals(block));
        Assert.Null(block.Txs);
    }

    // Can get blocks by hash which includes transactions (binary)
    [Fact(Skip = "Binary request not implemented")]
    public Task TestGetBlocksByHashBinary()
    {
        throw new MoneroError("Not implemented");
    }

    // Can get a block by height
    [Fact]
    public async Task TestGetBlockByHeight()
    {
        // config for testing blocks
        TestContext ctx = new();
        ctx.HasHex = true;
        ctx.HeaderIsFull = true;
        ctx.HasTxs = false;

        // retrieve by height of the last block
        MoneroBlockHeader lastHeader = await _daemon.GetLastBlockHeader();
        MoneroBlock block = await _daemon.GetBlockByHeight((ulong)lastHeader.GetHeight()!);
        TestBlock(block, ctx);
        Assert.True((await _daemon.GetBlockByHeight((ulong)block.GetHeight()!)).Equals(block));

        // retrieve by height of previous to last block
        block = await _daemon.GetBlockByHeight((ulong)lastHeader.GetHeight()! - 1);
        TestBlock(block, ctx);
        Assert.True(lastHeader.GetHeight() - 1 == (ulong)block.GetHeight()!);
    }

    // Can get a transaction by hash with and without pruning
    [Fact]
    public async Task TestGetTxByHash()
    {
        // fetch transaction hashes to test
        List<string> txHashes = await GetConfirmedTxHashes(_daemon);

        // context for testing txs
        TestContext ctx = new() { IsPruned = false, IsConfirmed = true, FromGetTxPool = false };

        // fetch each tx by hash without pruning
        if (txHashes.Count > 0)
        {
            List<MoneroTx> txs = await _daemon.GetTxs(txHashes, false);
            foreach (MoneroTx tx in txs)
            {
                TestTx(tx, ctx);
            }
        }

        // fetch each tx by hash with pruning
        if (txHashes.Count > 0)
        {
            List<MoneroTx> prunedTxs = await _daemon.GetTxs(txHashes, true);
            foreach (MoneroTx tx in prunedTxs)
            {
                ctx.IsPruned = true;
                TestTx(tx, ctx);
            }
        }

        // fetch invalid hash
        try
        {
            await _daemon.GetTxs(["invalid tx hash"], false);
            throw new MoneroError("fail");
        }
        catch (MoneroError e)
        {
            Assert.Equal("Invalid transaction hash", e.Message);
        }
    }

    // Can get a transaction hex by hash with and without pruning
    [Fact]
    public async Task TestGetTxHexByHash()
    {
        // fetch transaction hashes to test
        List<string> txHashes = await GetConfirmedTxHashes(_daemon);

        // fetch each tx hex by hash with and without pruning
        List<string> hexes = [];
        List<string> hexesPruned = [];
        if (txHashes.Count > 0)
        {
            hexes.AddRange(await _daemon.GetTxHexes(txHashes, false));
            hexesPruned.AddRange(await _daemon.GetTxHexes(txHashes, true));
        }

        // test results
        TestTxHexes(hexes, hexesPruned, txHashes);

        // fetch invalid hash
        try
        {
            await _daemon.GetTxHexes(["invalid tx hash"], false);
            throw new MoneroError("fail");
        }
        catch (MoneroError e)
        {
            Assert.Equal("Invalid transaction hash", e.Message);
        }
    }

    // Can get transaction hexes by hashes with and without pruning
    [Fact(Skip = "Needs monero-wallet-rpc")]
    public async Task TestGetTxHexesByHashes()
    {
        // fetch transaction hashes to test
        List<string> txHashes = await GetConfirmedTxHashes(_daemon);

        // fetch tx hexes by hash with and without pruning
        List<string> hexes = await _daemon.GetTxHexes(txHashes, false);
        List<string> hexesPruned = await _daemon.GetTxHexes(txHashes, true);

        // test results
        TestTxHexes(hexes, hexesPruned, txHashes);

        // fetch invalid hash
        txHashes.Add("invalid tx hash");
        try
        {
            await _daemon.GetTxHexes(txHashes, false);
            throw new MoneroError("fail");
        }
        catch (MoneroError e)
        {
            Assert.Equal("Invalid transaction hash", e.Message);
        }
    }

    // Can get the miner transaction sum
    [Fact(Skip = "Not supported by regtest daemon")]
    public async Task TestGetMinerTxSum()
    {
        MoneroMinerTxSum sum = await _daemon.GetMinerTxSum(0, Math.Min(50000, await _daemon.GetHeight()));
        TestMinerTxSum(sum);
    }

    // Can get a fee estimate
    [Fact]
    public async Task TestGetFeeEstimate()
    {
        MoneroFeeEstimate feeEstimate = await _daemon.GetFeeEstimate();
        TestUtils.TestUnsignedBigInteger(feeEstimate.Fee, true);
        Assert.Equal(4, feeEstimate.Fees?.Count); // slow, normal, fast, fastest
        for (int i = 0; i < 4; i++)
        {
            TestUtils.TestUnsignedBigInteger(feeEstimate?.Fees?[i], true);
        }

        TestUtils.TestUnsignedBigInteger(feeEstimate?.QuantizationMask, true);
    }

    // Can get hashes of transactions in the transaction pool (binary)
    [Fact(Skip = "Binary request not implemented")]
    public Task TestGetIdsOfTxsInPoolBin()
    {
        // TODO: get_transaction_pool_hashes.bin
        throw new MoneroError("Not implemented");
    }

    // Can get the transaction pool backlog (binary)
    [Fact(Skip = "Binary request not implemented")]
    public Task TestGetTxPoolBacklogBin()
    {
        // TODO: get_txpool_backlog
        throw new MoneroError("Not implemented");
    }

    // Can get output indices given a list of transaction hashes (binary)
    [Fact(Skip = "Binary request not implemented")]
    public Task TestGetOutputIndicesFromTxIdsBinary()
    {
        throw new Exception("Not implemented"); // get_o_indexes.bin
    }

    // Can get outputs given a list of output amounts and indices (binary)
    [Fact(Skip = "Binary request not implemented")]
    public Task TestGetOutputsFromAmountsAndIndicesBinary()
    {
        throw new Exception("Not implemented"); // get_outs.bin
    }

    // Can get an output histogram (binary)
    [Fact(Skip = "Binary request not implemented")]
    public async Task TestGetOutputHistogramBinary()
    {
        List<MoneroOutputHistogramEntry> entries = await
            _daemon.GetOutputHistogram([], null, null, null, null);
        Assert.True(entries.Count > 0);
        foreach (MoneroOutputHistogramEntry entry in entries)
        {
            TestOutputHistogramEntry(entry);
        }
    }

    // Can get an output distribution (binary)
    [Fact(Skip = "Binary request not implemented")]
    public async Task TestGetOutputDistributionBinary()
    {
        List<ulong> amounts = [];
        amounts.Add(0);
        amounts.Add(1);
        amounts.Add(10);
        amounts.Add(100);
        amounts.Add(1000);
        amounts.Add(10000);
        amounts.Add(100000);
        amounts.Add(1000000);
        List<MoneroOutputDistributionEntry> entries = await _daemon.GetOutputDistribution(amounts, false, null, null);
        foreach (MoneroOutputDistributionEntry entry in entries)
        {
            TestOutputDistributionEntry(entry);
        }
    }

    // Can get general information
    [Fact]
    public async Task TestGetGeneralInformation()
    {
        MoneroDaemonInfo info = await _daemon.GetInfo();
        TestInfo(info);
    }

    // Can get sync information
    [Fact]
    public async Task TestGetSyncInformation()
    {
        MoneroDaemonSyncInfo syncInfo = await _daemon.GetSyncInfo();
        TestSyncInfo(syncInfo);
    }

    // Can get hard fork information
    [Fact]
    public async Task TestGetHardForkInformation()
    {
        MoneroHardForkInfo hardForkInfo = await _daemon.GetHardForkInfo();
        TestHardForkInfo(hardForkInfo);
    }

    // Can get alternative chains
    [Fact]
    public async Task TestGetAlternativeChains()
    {
        List<MoneroAltChain> altChains = await _daemon.GetAltChains();
        foreach (MoneroAltChain altChain in altChains)
        {
            TestAltChain(altChain);
        }
    }

    // Can get alternative block hashes
    [Fact]
    public async Task TestGetAlternativeBlockIds()
    {
        List<string> altBlockIds = await _daemon.GetAltBlockHashes();
        foreach (string altBlockId in altBlockIds)
        {
            Assert.NotNull(altBlockId);
            Assert.Equal(64, altBlockId.Length); // TODO: common validation
        }
    }

    // Can get, set, and reset a download bandwidth limit
    [Fact]
    public async Task TestSetDownloadBandwidth()
    {
        int initVal = await _daemon.GetDownloadLimit();
        Assert.True(initVal > 0);
        int setVal = initVal * 2;
        await _daemon.SetDownloadLimit(setVal);
        Assert.True(setVal == await _daemon.GetDownloadLimit());
        int resetVal = await _daemon.ResetDownloadLimit();
        Assert.True(initVal == resetVal);

        // test invalid limits
        try
        {
            await _daemon.SetDownloadLimit(0);
            throw new MoneroError("Should have thrown error on invalid input");
        }
        catch (MoneroError e)
        {
            Assert.Equal("Download limit must be an integer greater than 0", e.Message);
        }

        Assert.True(await _daemon.GetDownloadLimit() == initVal);
    }

    // Can get, set, and reset an upload bandwidth limit
    [Fact]
    public async Task TestSetUploadBandwidth()
    {
        int initVal = await _daemon.GetUploadLimit();
        Assert.True(initVal > 0);
        int setVal = initVal * 2;
        await _daemon.SetUploadLimit(setVal);
        Assert.True(setVal == await _daemon.GetUploadLimit());
        int resetVal = await _daemon.ResetUploadLimit();
        Assert.True(initVal == resetVal);

        // test invalid limits
        try
        {
            await _daemon.SetUploadLimit(0);
            throw new Exception("Should have thrown error on invalid input");
        }
        catch (MoneroError e)
        {
            Assert.Equal("Upload limit must be an integer greater than 0", e.Message);
        }

        Assert.True(initVal == await _daemon.GetUploadLimit());
    }

    // Can get peers with active incoming or outgoing connections
    [Fact]
    public async Task TestGetPeers()
    {
        List<MoneroPeer> peers = await _daemon.GetPeers();
        Assert.True(peers.Count > 0, "Daemon has no incoming or outgoing peers to test");
        foreach (MoneroPeer peer in peers)
        {
            TestPeer(peer);
        }
    }

    // Can get all known peers that may be online or offline
    [Fact(Skip = "Daemon has no known peers to test")]
    public async Task TestGetKnownPeers()
    {
        List<MoneroPeer> peers = await _daemon.GetKnownPeers();
        Assert.True(peers.Count > 0, "Daemon has no known peers to test");
        foreach (MoneroPeer peer in peers)
        {
            TestKnownPeer(peer, false);
        }
    }

    // Can limit the number of outgoing peers
    [Fact]
    public async Task TestSetOutgoingPeerLimit()
    {
        await _daemon.SetOutgoingPeerLimit(0);
        await _daemon.SetOutgoingPeerLimit(8);
        await _daemon.SetOutgoingPeerLimit(10);
    }

    // Can limit the number of incoming peers
    [Fact]
    public async Task TestSetIncomingPeerLimit()
    {
        await _daemon.SetIncomingPeerLimit(0);
        await _daemon.SetIncomingPeerLimit(8);
        await _daemon.SetIncomingPeerLimit(10);
    }

    // Can ban a peer
    [Fact]
    public async Task TestBanPeer()
    {
        // set ban
        MoneroBan ban = new()
        {
            Host = "192.168.1.51",
            IsBanned = true,
            Seconds = 60
        };
        await _daemon.SetPeerBans([ban]);

        // test ban
        List<MoneroBan> bans = await _daemon.GetPeerBans();
        bool found = false;
        foreach (MoneroBan aBan in bans)
        {
            TestMoneroBan(aBan);
            if ("192.168.1.51".Equals(aBan.Host))
            {
                found = true;
            }
        }

        Assert.True(found);
    }

    // Can ban peers
    [Fact]
    public async Task TestBanPeers()
    {
        // set bans
        MoneroBan ban1 = new();
        ban1.Host = "192.168.1.52";
        ban1.IsBanned = true;
        ban1.Seconds = 60;
        MoneroBan ban2 = new();
        ban2.Host = "192.168.1.53";
        ban2.IsBanned = true;
        ban2.Seconds = 60;
        List<MoneroBan> bans = [];
        bans.Add(ban1);
        bans.Add(ban2);
        await _daemon.SetPeerBans(bans);

        // test bans
        bans = await _daemon.GetPeerBans();
        bool found1 = false;
        bool found2 = false;
        foreach (MoneroBan aBan in bans)
        {
            TestMoneroBan(aBan);
            if ("192.168.1.52".Equals(aBan.Host))
            {
                found1 = true;
            }

            if ("192.168.1.53".Equals(aBan.Host))
            {
                found2 = true;
            }
        }

        Assert.True(found1);
        Assert.True(found2);
    }

    // Can start and stop mining
    [Fact(Skip = "Fails on github CI")]
    public async Task TestMining()
    {
        // stop mining at the beginning of the test
        try { await _daemon.StopMining(); }
        catch (MoneroError)
        {
            // ignore
        }

        // generate address to mine to
        // TODO use wallet rpc
        string address = TestUtils.Address;
        // start mining
        await _daemon.StartMining(address, 1, false, true);

        GenUtils.WaitFor(30);

        // stop mining
        await _daemon.StopMining();
    }

    // Can get mining status
    // TODO why this test fails on github runner?
    [Fact(Skip = "Fails on github CI")]
    public async Task TestGetMiningStatus()
    {
        try
        {
            // stop mining at the beginning of the test
            try { await _daemon.StopMining(); }
            catch (MoneroError)
            {
                // ignore
            }

            // test status without mining
            MoneroMiningStatus status = await _daemon.GetMiningStatus();
            Assert.False(status.IsActive);
            Assert.Null(status.Address);
            Assert.Equal(0, (long)status.Speed!);
            Assert.Equal(0, (int)status.NumThreads!);
            Assert.Null(status.IsBackground);

            // test status with mining
            // TODO use wallet rpc address
            string address = TestUtils.Address;
            ulong threadCount = 1;
            bool isBackground = false;
            await _daemon.StartMining(address, threadCount, isBackground, true);
            status = await _daemon.GetMiningStatus();
            Assert.True(status.IsActive);
            Assert.True(address == status.Address);
            Assert.True(status.Speed >= 0);
            Assert.True(threadCount == status.NumThreads);
            Assert.True(isBackground == status.IsBackground);
        }
        finally
        {
            // stop mining at the end of the test
            try { await _daemon.StopMining(); }
            catch (MoneroError)
            {
                // ignore
            }
        }
    }

    // Can submit a mined block to the network
    [Fact(Skip = "Not supported by regtest daemon")]
    public async Task TestSubmitMinedBlock()
    {
        // get template to mine on
        MoneroBlockTemplate template = await _daemon.GetBlockTemplate(TestUtils.Address, 0);

        // TODO monero rpc: way to get mining nonce when found in order to submit?

        // try to submit a block hashing blob without nonce
        try
        {
            await _daemon.SubmitBlocks([template.BlockTemplateBlob!]);
            throw new Exception("Should have thrown error");
        }
        catch (MoneroRpcError e)
        {
            Assert.True(-7 == e.GetCode());
            Assert.Equal("Block not accepted", e.Message);
        }
    }

    // Can prune the blockchain
    [Fact(Skip = "Not supported by regtest daemon")]
    public async Task TestPruneBlockchain()
    {
        MoneroPruneResult result = await _daemon.PruneBlockchain(true);
        if (result.IsPruned == true)
        {
            Assert.True(result.PruningSeed > 0);
        }
        else
        {
            Assert.True(0 == result.PruningSeed);
        }
    }

    // Can check for an update
    [Fact(Skip = "Unstable update call")]
    public async Task TestCheckForUpdate()
    {
        MoneroDaemonUpdateCheckResult result = await _daemon.CheckForUpdate();
        TestUpdateCheckResult(result);
    }

    // Can download an update
    [Fact(Skip = "Non supported by regtest daemon")]
    public async Task TestDownloadUpdate()
    {
        // download to a default path
        MoneroDaemonUpdateDownloadResult result = await _daemon.DownloadUpdate("");
        TestUpdateDownloadResult(result, null);

        // download to a defined path
        string path = "test_download_" + DateTime.Now + ".tar.bz2";
        result = await _daemon.DownloadUpdate(path);
        TestUpdateDownloadResult(result, path);

        // test invalid path
        if (result.IsUpdateAvailable == true)
        {
            try
            {
                await _daemon.DownloadUpdate("./ohhai/there");
                throw new Exception("Should have thrown error");
            }
            catch (MoneroRpcError e)
            {
                Assert.NotEqual("Should have thrown error", e.Message);
                Assert.Equal(500, e.GetCode()); // TODO monerod: this causes a 500 in daemon rpc
            }
        }
    }

    // Can be stopped
    [Fact(Skip = "Disabled")]
    public async Task TestStop()
    {
        // stop the daemon
        await _daemon.Stop();

        // give the daemon time to shut down
        GenUtils.WaitFor(TestUtils.SyncPeriodInMs);
        // try to interact with the daemon
        try
        {
            await _daemon.GetHeight();
            throw new Exception("Should have thrown error");
        }
        catch (MoneroError e)
        {
            Assert.NotEqual("Should have thrown error", e.Message);
        }
    }

    #endregion

    #region Test Helpers

    private static void TestBlockHeader(MoneroBlockHeader? header, bool isFull)
    {
        Assert.NotNull(header);
        Assert.True(header.GetHeight() >= 0);
        Assert.True(header.GetMajorVersion() > 0);
        Assert.True(header.GetMinorVersion() >= 0);
        if (header.GetHeight() == 0)
        {
            Assert.True(header.GetTimestamp() == 0);
        }
        else
        {
            Assert.True(header.GetTimestamp() > 0);
        }

        Assert.NotNull(header.GetPrevHash());
        Assert.NotNull(header.GetNonce());
        if (header.GetNonce() == 0)
        {
            MoneroUtils.Log(0,
                "WARNING: header nonce is 0 at height " +
                header.GetHeight()); // TODO (monero-project): why is header nonce 0?
        }
        else
        {
            Assert.True(header.GetNonce() > 0);
        }

        Assert.NotNull(header.GetPowHash()); // never seen defined
        if (isFull)
        {
            Assert.True(header.GetSize() > 0);
            Assert.True(header.GetDepth() >= 0);
            Assert.True(header.GetDifficulty() > 0);
            Assert.True(header.GetCumulativeDifficulty() > 0);
            Assert.Equal(64, header.GetHash()!.Length);
            Assert.Equal(64, header.GetMinerTxHash()!.Length);
            Assert.True(header.GetNumTxs() >= 0);
            Assert.NotNull(header.GetOrphanStatus());
            Assert.NotNull(header.GetReward());
            Assert.NotNull(header.GetWeight());
            Assert.True(header.GetWeight() > 0);
        }
        else
        {
            Assert.Null(header.GetSize());
            Assert.Null(header.GetDepth());
            Assert.Null(header.GetDifficulty());
            Assert.Null(header.GetCumulativeDifficulty());
            Assert.Null(header.GetHash());
            Assert.Null(header.GetMinerTxHash());
            Assert.Null(header.GetNumTxs());
            Assert.Null(header.GetOrphanStatus());
            Assert.Null(header.GetReward());
            Assert.Null(header.GetWeight());
        }
    }

    private static async Task<List<string>> GetConfirmedTxHashes(IMoneroDaemon daemon)
    {
        const int numTxs = 5;
        List<string> txHashes = [];
        ulong height = await daemon.GetHeight();
        while (txHashes.Count < numTxs && height > 0)
        {
            MoneroBlock block = await daemon.GetBlockByHeight(--height);
            txHashes.AddRange(block.TxHashes);
        }

        return txHashes;
    }

    private static void TestBlockTemplate(MoneroBlockTemplate template)
    {
        Assert.NotNull(template);
        Assert.NotNull(template.BlockTemplateBlob);
        Assert.NotNull(template.BlockHashingBlob);
        Assert.NotNull(template.Difficulty);
        Assert.NotNull(template.ExpectedReward);
        Assert.NotNull(template.Height);
        Assert.NotNull(template.PrevHash);
        Assert.NotNull(template.ReservedOffset);
        Assert.NotNull(template.SeedHeight);
        // regtest daemon has seed height equal to zero
        Assert.NotNull(template.SeedHash);
        Assert.True(template.SeedHash.Length > 0);
        // next seed hash can be null or initialized  // TODO: test circumstances for each
    }

    // TODO: test block deep copy
    private static void TestBlock(MoneroBlock block, TestContext ctx)
    {
        // test required fields
        Assert.NotNull(block);
        TestBlockHeader(block, ctx.HeaderIsFull == true);

        if (ctx.HasHex == true)
        {
            Assert.NotNull(block.Hex);
            Assert.True(block.Hex!.Length > 1);
        }
        else
        {
            Assert.NotNull(block.Hex);
        }

        if (ctx.HasTxs == true)
        {
            Assert.NotNull(ctx.TxContext);
            foreach (MoneroTx tx in block.Txs!)
            {
                Assert.True(block.Equals(tx.GetBlock()));
                TestTx(tx, ctx.TxContext);
            }
        }
        else
        {
            Assert.Null(ctx.TxContext);
            Assert.Null(block.Txs);
        }
    }

    private static void TestTx(MoneroTx? tx, TestContext? ctx)
    {
        // check inputs
        Assert.NotNull(tx);
        Assert.NotNull(ctx);
        Assert.NotNull(ctx.IsPruned);
        Assert.NotNull(ctx.IsConfirmed);
        Assert.NotNull(ctx.FromGetTxPool);

        // standard across all txs
        Assert.Equal(64, tx.GetHash()!.Length);
        if (tx.IsRelayed() == null)
        {
            Assert.True(tx.InTxPool()); // TODO monerod: add relayed to get_transactions
        }
        else
        {
            Assert.NotNull(tx.IsRelayed());
        }

        Assert.NotNull(tx.IsConfirmed());
        Assert.NotNull(tx.InTxPool());
        Assert.NotNull(tx.IsMinerTx());
        Assert.NotNull(tx.IsDoubleSpendSeen());
        Assert.True(tx.GetVersion() >= 0);
        Assert.True(tx.GetUnlockTime() >= 0);
        Assert.NotNull(tx.GetInputs());
        Assert.NotNull(tx.GetOutputs());
        Assert.True(tx.GetExtra()!.Length > 0);
        TestUtils.TestUnsignedBigInteger(tx.GetFee(), true);

        // test presence of output indices
        // TODO: change this over to outputs only
        if (tx.IsMinerTx() == true)
        {
            Assert.Null(tx.GetOutputIndices()); // TODO: how to get output indices for miner transactions?
        }

        if (tx.InTxPool() == true || ctx.FromGetTxPool == true || ctx.HasOutputIndices == false)
        {
            Assert.Null(tx.GetOutputIndices());
        }
        else
        {
            Assert.NotNull(tx.GetOutputIndices());
        }

        if (tx.GetOutputIndices() != null)
        {
            Assert.True(tx.GetOutputIndices()!.Count > 0);
        }

        // test confirmed ctx
        if (ctx.IsConfirmed == true)
        {
            Assert.True(tx.IsConfirmed());
        }

        if (ctx.IsConfirmed == false)
        {
            Assert.False(tx.IsConfirmed());
        }

        // test confirmed
        if (tx.IsConfirmed() == true)
        {
            Assert.NotNull(tx.GetBlock());
            Assert.Contains(tx, tx.GetBlock()!.Txs!);
            Assert.True(tx.GetBlock()!.GetHeight() > 0);
            Assert.Contains(tx, tx.GetBlock()!.Txs!);
            Assert.True(tx.GetBlock()!.GetHeight() > 0);
            Assert.True(tx.GetBlock()!.GetTimestamp() > 0);
            Assert.True(tx.GetRelay());
            Assert.True(tx.IsRelayed());
            Assert.False(tx.IsFailed());
            Assert.False(tx.InTxPool());
            Assert.False(tx.IsDoubleSpendSeen());
            if (ctx.FromBinaryBlock == true)
            {
                Assert.Null(tx.GetNumConfirmations());
            }
            else
            {
                Assert.True(tx.GetNumConfirmations() > 0);
            }
        }
        else
        {
            Assert.Null(tx.GetBlock());
            Assert.Equal(0, (long)tx.GetNumConfirmations()!);
        }

        // test in tx pool
        if (tx.InTxPool() == true)
        {
            Assert.False(tx.IsConfirmed());
            Assert.False(tx.IsDoubleSpendSeen());
            Assert.Null(tx.GetLastFailedHeight());
            Assert.Null(tx.GetLastFailedHash());
            Assert.True(tx.GetReceivedTimestamp() > 0);
            if (ctx.FromGetTxPool == true)
            {
                Assert.True(tx.GetSize() > 0);
                Assert.True(tx.GetWeight() > 0);
                Assert.NotNull(tx.IsKeptByBlock());
                Assert.True(tx.GetMaxUsedBlockHeight() >= 0);
                Assert.NotNull(tx.GetMaxUsedBlockHash());
            }

            Assert.Null(tx.GetLastFailedHeight());
            Assert.Null(tx.GetLastFailedHash());
        }
        else
        {
            Assert.Null(tx.GetLastRelayedTimestamp());
        }

        // test miner tx
        if (tx.IsMinerTx() == true)
        {
            Assert.True(0 == tx.GetFee());
            Assert.Null(tx.GetInputs());
            Assert.Null(tx.GetSignatures());
        }
        else
        {
            if (tx.GetSignatures() != null)
            {
                Assert.True(tx.GetSignatures()!.Count > 0);
            }
        }

        // test failed
        // TODO: what else to test associated with failed
        if (tx.IsFailed() == true)
        {
            Assert.True(tx.GetReceivedTimestamp() > 0);
        }
        else
        {
            if (tx.IsRelayed() == null)
            {
                Assert.Null(tx.GetRelay()); // TODO monerod: add relayed to get_transactions
            }
            else if (tx.IsRelayed() == true)
            {
                Assert.False(tx.IsDoubleSpendSeen());
            }
            else
            {
                Assert.False(tx.IsRelayed());
                if (ctx.FromGetTxPool == true)
                {
                    Assert.False(tx.GetRelay());
                    Assert.NotNull(tx.IsDoubleSpendSeen());
                }
            }
        }

        Assert.Null(tx.GetLastFailedHeight());
        Assert.Null(tx.GetLastFailedHash());

        // received time only for tx pool or failed txs
        if (tx.GetReceivedTimestamp() != null)
        {
            Assert.True(tx.InTxPool() == true || tx.IsFailed() == true);
        }

        // test inputs and outputs
        if (tx.IsMinerTx() == false)
        {
            Assert.True(tx.GetInputs()!.Count > 0);
        }

        foreach (MoneroOutput input in tx.GetInputs()!)
        {
            Assert.True(tx == input.GetTx());
            TestInput(input);
        }

        Assert.True(tx.GetOutputs()!.Count > 0);
        foreach (MoneroOutput output in tx.GetOutputs()!)
        {
            Assert.True(tx == output.GetTx());
            TestOutput(output, ctx);
        }

        // test pruned vs not pruned
        if (ctx.FromGetTxPool == true || ctx.FromBinaryBlock == true)
        {
            Assert.Null(tx
                .GetPrunableHash()); // TODO monerod: tx pool txs do not have prunable hash, TODO: GetBlocksByHeight() has inconsistent client-side pruning
        }
        else
        {
            Assert.NotNull(tx.GetPrunableHash());
        }

        if (ctx.IsPruned == true)
        {
            Assert.Null(tx.GetRctSigPrunable());
            Assert.Null(tx.GetSize());
            Assert.Null(tx.GetLastRelayedTimestamp());
            Assert.Null(tx.GetReceivedTimestamp());
            Assert.Null(tx.GetFullHex());
            Assert.NotNull(tx.GetPrunedHex());
        }
        else
        {
            Assert.Null(tx.GetPrunedHex());
            if (ctx.FromBinaryBlock == true)
            {
                Assert.Null(tx.GetFullHex()); // TODO: GetBlocksByHeight() has inconsistent client-side pruning
            }
            else
            {
                Assert.True(tx.GetFullHex()!.Length > 0);
            }

            if (ctx.FromBinaryBlock == true)
            {
                Assert.Null(tx.GetRctSigPrunable()); // TODO: GetBlocksByHeight() has inconsistent client-side pruning
            }

            //else Assert.NotNull((tx.GetRctSigPrunable()); // TODO: define and test this
            Assert.False(tx.IsDoubleSpendSeen());
            if (tx.IsConfirmed() == true)
            {
                Assert.Null(tx.GetLastRelayedTimestamp());
                Assert.Null(tx.GetReceivedTimestamp());
            }
            else
            {
                if (tx.IsRelayed() == true)
                {
                    Assert.True(tx.GetLastRelayedTimestamp() > 0);
                }
                else
                {
                    Assert.Null(tx.GetLastRelayedTimestamp());
                }

                Assert.True(tx.GetReceivedTimestamp() > 0);
            }
        }

        if (tx.IsFailed() == true)
        {
            // TODO: implement this
        }

        // test deep copy
        if (true != ctx.DoNotTestCopy)
        {
            TestTxCopy(tx, ctx);
        }
    }

    private static void TestInput(MoneroOutput input)
    {
        TestOutput(input);
        TestKeyImage(input.GetKeyImage()!);
        Assert.True(input.GetRingOutputIndices()!.Count > 0);
    }

    private static void TestKeyImage(MoneroKeyImage image)
    {
        Assert.True(image.Hex!.Length > 0);
        if (image.Signature != null)
        {
            Assert.NotNull(image.Signature);
            Assert.True(image.Signature!.Length > 0);
        }
    }

    private static void TestOutput(MoneroOutput output, TestContext ctx)
    {
        TestOutput(output);
        if (output.GetTx()!.InTxPool() == true || ctx.HasOutputIndices == false)
        {
            Assert.Null(output.GetIndex());
        }
        else
        {
            Assert.True(output.GetIndex() >= 0);
        }

        Assert.Equal(64, output.GetStealthPublicKey()!.Length);
    }

    private static void TestOutput(MoneroOutput output)
    {
        TestUtils.TestUnsignedBigInteger(output.GetAmount());
    }

    private static void TestTxCopy(MoneroTx tx, TestContext ctx)
    {
        // copy tx and assert deep equality
        MoneroTx copy = tx.Clone();
        Assert.Null(copy.GetBlock());

        Assert.True(tx.ToString() == copy.ToString());
        Assert.True(copy != tx);

        // test different input references
        if (copy.GetInputs() == null)
        {
            Assert.Null(tx.GetInputs());
        }
        else
        {
            Assert.True(copy.GetInputs() != tx.GetInputs());
            for (int i = 0; i < copy.GetInputs()!.Count; i++)
            {
                Assert.True(tx.GetInputs()![i].GetAmount().Equals(copy.GetInputs()![i].GetAmount()));
            }
        }

        // test different output references
        if (copy.GetOutputs() == null)
        {
            Assert.Null(tx.GetOutputs());
        }
        else
        {
            Assert.True(copy.GetOutputs() != tx.GetOutputs());
            for (int i = 0; i < copy.GetOutputs()!.Count; i++)
            {
                Assert.True(tx.GetOutputs()![i].GetAmount().Equals(copy.GetOutputs()![i].GetAmount()));
            }
        }

        // test copied tx
        ctx = new TestContext(ctx)
        {
            DoNotTestCopy = true // to prevent infinite recursion
        };

        TestTx(copy, ctx);
    }

    private static void TestMinerTxSum(MoneroMinerTxSum txSum)
    {
        TestUtils.TestUnsignedBigInteger(txSum.EmissionSum, true);
        TestUtils.TestUnsignedBigInteger(txSum.FeeSum, true);
    }

    private static void TestOutputDistributionEntry(MoneroOutputDistributionEntry entry)
    {
        TestUtils.TestUnsignedBigInteger(entry.Amount);
        Assert.True(entry.Base >= 0);
        Assert.True(entry.Distribution!.Count > 0);
        Assert.True(entry.StartHeight >= 0);
    }

    private static void TestInfo(MoneroDaemonInfo info)
    {
        Assert.NotNull(info.Version);
        Assert.True(info.NumAltBlocks >= 0);
        Assert.True(info.BlockSizeLimit > 0);
        Assert.True(info.BlockSizeMedian > 0);
        TestUtils.TestUnsignedBigInteger(info.CumulativeDifficulty);
        TestUtils.TestUnsignedBigInteger(info.FreeSpace);
        Assert.True(info.NumOfflinePeers >= 0);
        Assert.True(info.NumOnlinePeers >= 0);
        Assert.True(info.Height >= 0);
        Assert.True(info.NumIncomingConnections >= 0);
        Assert.True(info.NumOutgoingConnections >= 0);
        Assert.True(info.NumRpcConnections >= 0);
        Assert.True(info.AdjustedTimestamp > 0);
        Assert.True(info.Target > 0);
        Assert.True(info.TargetHeight >= 0);
        Assert.True(info.NumTxs >= 0);
        Assert.True(info.NumTxsPool >= 0);
        Assert.NotNull(info.WasBootstrapEverUsed);
        Assert.True(info.BlockWeightLimit > 0);
        Assert.True(info.BlockWeightMedian > 0);
        Assert.True(info.DatabaseSize > 0);
        Assert.NotNull(info.UpdateAvailable);
        TestUtils.TestUnsignedBigInteger(info.Credits, false); // 0 credits
        string? topBlockHash = info.TopBlockHash;
        Assert.NotNull(topBlockHash);
        Assert.True(topBlockHash.Length > 0);

        if (info.IsRestricted)
        {
            return;
        }

        Assert.True(info.HeightWithoutBootstrap > 0);
        Assert.True(info.StartTimestamp > 0);
    }

    private static void TestSyncInfo(MoneroDaemonSyncInfo syncInfo)
    {
        // TODO: consistent naming, daemon in name?
        MoneroDaemonSyncInfo testObj = new();

        Assert.True(testObj.GetType().IsInstanceOfType(syncInfo));
        Assert.True(syncInfo.Height >= 0);
        if (syncInfo.Peers != null)
        {
            Assert.True(syncInfo.Peers!.Count > 0);
            foreach (MoneroPeerInfo connection in syncInfo.Peers!)
            {
                TestPeer(connection.Info!);
            }
        }

        if (syncInfo.Spans != null)
        {
            // TODO: test that this is being hit, so far not used
            Assert.True(syncInfo.Spans!.Count > 0);
            foreach (MoneroConnectionSpan span in syncInfo.Spans!)
            {
                TestConnectionSpan(span);
            }
        }

        Assert.True(syncInfo.NextNeededPruningSeed >= 0);
        Assert.NotNull(syncInfo.Overview);
        TestUtils.TestUnsignedBigInteger(syncInfo.Credits, false); // 0 credits
        Assert.Null(syncInfo.TopBlockHash);
    }

    private static void TestConnectionSpan(MoneroConnectionSpan? span)
    {
        Assert.NotNull(span);
        Assert.NotNull(span.ConnectionId);
        Assert.True(span.ConnectionId!.Length > 0);
        Assert.True(span.StartHeight > 0);
        Assert.True(span.NumBlocks > 0);
        Assert.True(span.RemoteAddress == null || span.RemoteAddress!.Length > 0);
        Assert.True(span.Rate > 0);
        Assert.True(span.Speed >= 0);
        Assert.True(span.Size > 0);
    }

    private static void TestHardForkInfo(MoneroHardForkInfo hardForkInfo)
    {
        Assert.NotNull(hardForkInfo.EarliestHeight);
        Assert.NotNull(hardForkInfo.IsEnabled);
        Assert.NotNull(hardForkInfo.State);
        Assert.NotNull(hardForkInfo.Threshold);
        Assert.NotNull(hardForkInfo.Version);
        Assert.NotNull(hardForkInfo.NumVotes);
        Assert.NotNull(hardForkInfo.Voting);
        Assert.NotNull(hardForkInfo.Window);
        TestUtils.TestUnsignedBigInteger(hardForkInfo.Credits, false); // 0 credits
        Assert.Null(hardForkInfo.TopBlockHash);
    }

    private static void TestMoneroBan(MoneroBan ban)
    {
        Assert.NotNull(ban.Host);
        Assert.NotNull(ban.Ip);
        Assert.NotNull(ban.Seconds);
    }

    private static void TestAltChain(MoneroAltChain altChain)
    {
        Assert.NotNull(altChain);
        Assert.True(altChain.BlockHashes!.Count > 0);
        TestUtils.TestUnsignedBigInteger(altChain.Difficulty, true);
        Assert.True(altChain.Height > 0);
        Assert.True(altChain.Length > 0);
        Assert.Equal(64, altChain.MainChainParentBlockHash!.Length);
    }

    private static void TestPeer(MoneroPeer peer)
    {
        TestKnownPeer(peer, true);
        Assert.True(peer.Hash!.Length > 0);
        Assert.True(peer.AvgDownload >= 0);
        Assert.True(peer.AvgUpload >= 0);
        Assert.True(peer.CurrentDownload >= 0);
        Assert.True(peer.CurrentUpload >= 0);
        Assert.True(peer.Height >= 0);
        Assert.True(peer.LiveTime >= 0);
        Assert.True(peer.NumReceives >= 0);
        Assert.True(peer.ReceiveIdleTime >= 0);
        Assert.True(peer.NumSends >= 0);
        Assert.True(peer.SendIdleTime >= 0);
        Assert.NotNull(peer.State);
        Assert.True(peer.NumSupportFlags >= 0);
    }

    private static void TestKnownPeer(MoneroPeer peer, bool fromConnection)
    {
        Assert.NotNull(peer);
        Assert.True(peer.Port != null && int.Parse(peer.Port) > 0);
        Assert.True(peer.RpcPort is null or >= 0);
        if (fromConnection)
        {
            Assert.Null(peer.LastSeenTimestamp);
        }
        else
        {
            if (peer.LastSeenTimestamp < 0)
            {
                MoneroUtils.Log(0, "Last seen timestamp is invalid: " + peer.LastSeenTimestamp);
            }

            Assert.True(peer.LastSeenTimestamp >= 0);
        }

        Assert.True(peer.PruningSeed == null || peer.PruningSeed >= 0);
    }

    private static void TestUpdateCheckResult(MoneroDaemonUpdateCheckResult result)
    {
        Assert.NotNull(result.IsUpdateAvailable);
        if (result.IsUpdateAvailable == true)
        {
            Assert.True(result.AutoUri!.Length > 0, "No auto uri; is daemon online?");
            Assert.True(result.UserUri!.Length > 0);
            Assert.True(result.Version!.Length > 0);
            Assert.True(result.Hash!.Length > 0);
            Assert.Equal(64, result.Hash!.Length);
        }
        else
        {
            Assert.Null(result.AutoUri);
            Assert.Null(result.UserUri);
            Assert.Null(result.Version);
            Assert.Null(result.Hash);
        }
    }

    private static void TestUpdateDownloadResult(MoneroDaemonUpdateDownloadResult result, string? path)
    {
        TestUpdateCheckResult(result);
        if (result.IsUpdateAvailable == true)
        {
            if (path != null)
            {
                Assert.True(path == result.DownloadPath);
            }
            else
            {
                Assert.NotNull(result.DownloadPath);
            }
        }
        else
        {
            Assert.Null(result.DownloadPath);
        }
    }

    private static void TestOutputHistogramEntry(MoneroOutputHistogramEntry entry)
    {
        TestUtils.TestUnsignedBigInteger(entry.Amount);
        Assert.True(entry.NumInstances >= 0);
        Assert.True(entry.NumUnlockedInstances >= 0);
        Assert.True(entry.NumRecentInstances >= 0);
    }

    private static void TestTxHexes(List<string> hexes, List<string> hexesPruned, List<string> txHashes)
    {
        Assert.True(hexes.Count == txHashes.Count);
        Assert.True(hexesPruned.Count == txHashes.Count);
        for (int i = 0; i < hexes.Count; i++)
        {
            Assert.NotNull(hexes[i]);
            Assert.NotNull(hexesPruned[i]);
            Assert.True(hexesPruned.Count > 0);
            Assert.True(hexes[i].Length > hexesPruned[i].Length); // pruned hex is shorter
        }
    }

    #endregion
}

public class TestContext
{
    public bool? DoNotTestCopy;
    public bool? FromBinaryBlock;
    public bool? FromGetTxPool;
    public bool? HasHex;
    public bool? HasJson;
    public bool? HasOutputIndices;
    public bool? HasTxs;
    public bool? HeaderIsFull;
    public bool? IsConfirmed;
    public bool? IsFull;
    public bool? IsMinerTx;
    public bool? IsPruned;
    public TestContext? TxContext;

    public TestContext() { }

    public TestContext(TestContext ctx)
    {
        HasJson = ctx.HasJson;
        IsPruned = ctx.IsPruned;
        IsFull = ctx.IsFull;
        IsConfirmed = ctx.IsConfirmed;
        IsMinerTx = ctx.IsMinerTx;
        FromGetTxPool = ctx.FromGetTxPool;
        FromBinaryBlock = ctx.FromBinaryBlock;
        HasOutputIndices = ctx.HasOutputIndices;
        DoNotTestCopy = ctx.DoNotTestCopy;
        HasTxs = ctx.HasTxs;
        HasHex = ctx.HasHex;
        HeaderIsFull = ctx.HeaderIsFull;
    }
}