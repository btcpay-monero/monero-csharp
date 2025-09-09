using Monero.Common;
using Monero.Wallet.Common;

namespace Monero.Wallet;

public interface MoneroWallet
{
    public static readonly string DEFAULT_LANGUAGE = "English";

    public MoneroWalletType GetWalletType();

    /**
    * Register a listener to receive wallet notifications.
    *
    * @param listener is the listener to receive wallet notifications
    */
    public void AddListener(MoneroWalletListener listener);

    /**
     * Unregister a listener to receive wallet notifications.
     *
     * @param listener is the listener to unregister
     */
    public void RemoveListener(MoneroWalletListener listener);

    /**
     * Get the listeners registered with the wallet.
     *
     * @return the registered listeners
     */
    public List<MoneroWalletListener> GetListeners();

    /**
     * Indicates if the wallet is view-only, meaning it does not have the private
     * spend key and can therefore only observe incoming outputs.
     *
     * @return {bool} true if the wallet is view-only, false otherwise
     */
    public bool IsViewOnly();

    /**
     * Set the wallet's daemon connection.
     *
     * @param uri is the daemon's URI
     * @param username is the username to authenticate with the daemon (optional)
     * @param password is the password to authenticate with the daemon (optional)
     */
    public void SetDaemonConnection(string? uri, string? username = null, string? password = null);

    /**
     * Set the wallet's daemon connection
     *
     * @param daemonConnection manages daemon connection information
     */
    public void SetDaemonConnection(MoneroRpcConnection? daemonConnection);

    /**
     * Get the wallet's daemon connection.
     *
     * @return the wallet's daemon connection
     */
    public MoneroRpcConnection? GetDaemonConnection();

    /**
     * Set the wallet's daemon connection manager.
     *
     * @param connectionManager manages connections to monerod
     */
    public void SetConnectionManager(MoneroConnectionManager? connectionManager);

    /**
     * Get the wallet's daemon connection manager.
     *
     * @return the wallet's daemon connection manager
     */
    public MoneroConnectionManager? GetConnectionManager();

    /**
     * Set the Tor proxy to the daemon.
     *
     * @param uri the Tor proxy URI
     */
    public void SetProxyUri(string? uri);

    /**
     * Indicates if the wallet is connected a daemon.
     *
     * @return true if the wallet is connected to a daemon, false otherwise
     */
    public bool IsConnectedToDaemon();

    /**
     * Returns the wallet version.
     *
     * @return the wallet version
     */
    public MoneroVersion GetVersion();

    public MoneroNetworkType GetNetworkType();

    /**
     * Get the wallet's path.
     *
     * @return the path the wallet can be opened with
     */
    public string GetPath();

    /**
     * Get the wallet's mnemonic phrase or seed.
     *
     * @return the wallet's mnemonic phrase or seed.
     */
    public string GetSeed();

    /**
     * Get the language of the wallet's mnemonic phrase or seed.
     *
     * @return the language of the wallet's mnemonic phrase or seed
     */
    public string GetSeedLanguage();

    /**
     * Get the wallet's private view key.
     *
     * @return the wallet's private view key
     */
    public string GetPrivateViewKey();

    /**
     * Get the wallet's private spend key.
     *
     * @return the wallet's private spend key
     */
    public string GetPrivateSpendKey();

    /**
     * Get the wallet's public view key.
     *
     * @return the wallet's public view key
     */
    public string GetPublicViewKey();

    /**
     * Get the wallet's public spend key.
     *
     * @return the wallet's public spend key
     */
    public string GetPublicSpendKey();

    /**
     * Get the wallet's primary address.
     *
     * @return the wallet's primary address
     */
    public string GetPrimaryAddress();

    /**
     * Get the address of a specific subaddress.
     *
     * @param accountIdx specifies the account index of the address's subaddress
     * @param subaddressIdx specifies the subaddress index within the account
     * @return the receive address of the specified subaddress
     */
    public string GetAddress(uint accountIdx, uint subaddressIdx);

    /**
     * Get the account and subaddress index of the given address.
     *
     * @param address is the address to Get the account and subaddress index from
     * @return the account and subaddress indices
     */
    public MoneroSubaddress GetAddressIndex(string address);

    /**
     * Get an integrated address based on the given standard address and payment
     * ID. Uses the wallet's primary address if an address is not given.
     * Generates a random payment ID if a payment ID is not given.
     *
     * @param standardAddress is the standard address to generate the integrated address from (wallet's primary address if null)
     * @param paymentId is the payment ID to generate an integrated address from (randomly generated if null)
     * @return the integrated address
     */
    public MoneroIntegratedAddress GetIntegratedAddress(string? standardAddress = null, string? paymentId = null);

    /**
     * Decode an integrated address to Get its standard address and payment id.
     *
     * @param integratedAddress is an integrated address to decode
     * @return the decoded integrated address including standard address and payment id
     */
    public MoneroIntegratedAddress DecodeIntegratedAddress(string integratedAddress);

    /**
     * Get the block height that the wallet is Synced to.
     *
     * @return the block height that the wallet is Synced to
     */
    public ulong GetHeight();

    /**
     * Get the blockchain's height.
     *
     * @return the blockchain's height
     */
    public ulong GetDaemonHeight();

    /**
     * Get the blockchain's height by date as a conservative estimate for scanning.
     *
     * @param year year of the height to Get
     * @param month month of the height to Get as a number between 1 and 12
     * @param day day of the height to Get as a number between 1 and 31
     * @return the blockchain's approximate height at the given date
     */
    public ulong GetHeightByDate(int year, int month, int day);

    /**
     * Synchronize the wallet with the daemon as a one-time Synchronous process.
     *
     * @param listener listener to receive notifications during Synchronization
     * @return the Sync result
     */
    public MoneroSyncResult Sync(MoneroWalletListener listener);

    /**
     * Synchronize the wallet with the daemon as a one-time Synchronous process.
     *
     * @param startHeight is the start height to Sync from (defaults to the last Synced block)
     * @param listener listener to receive notifications during Synchronization
     * @return the Sync result
     */
    public MoneroSyncResult Sync(ulong? startHeight = null, MoneroWalletListener? listener = null);

    /**
     * Start background Synchronizing with a maximum period between Syncs.
     *
     * @param SyncPeriodInMs maximum period between Syncs in milliseconds
     */
    public void StartSyncing(ulong? SyncPeriodInMs = null);

    /**
     * Stop Synchronizing the wallet with the daemon.
     */
    public void StopSyncing();

    /**
     * Scan transactions by their hash/id.
     *
     * @param txHashes tx hashes to scan
     */
    public void ScanTxs(List<string> txHashes);

    /**
     * Rescan the blockchain for spent outputs.
     *
     * Note: this can only be called with a trusted daemon.
     *
     * Example use case: peer multisig hex is import when connected to an untrusted daemon,
     * so the wallet will not rescan spent outputs.  Then the wallet connects to a trusted
     * daemon.  This method should be manually invoked to rescan outputs.
     */
    public void RescanSpent();

    /**
     * Rescan the blockchain from scratch, losing any information which cannot be recovered from
     * the blockchain itself.
     *
     * WARNING: This method discards local wallet data like destination addresses, tx secret keys,
     * tx notes, etc.
     */
    public void RescanBlockchain();

    /**
     * Get a subaddress's balance.
     *
     * @param accountIdx index of the account to Get the balance of (default all accounts if null)
     * @param subaddressIdx index of the subaddress to Get the balance of (default all subaddresses if null)
     * @return the requested balance
     */
    public ulong GetBalance(uint? accountIdx = null, uint? subaddressIdx = null);

    /**
     * Get a subaddress's unlocked balance.
     *
     * @param accountIdx index of the subaddress to Get the unlocked balance of (default all accounts if null)
     * @param subaddressIdx index of the subaddress to Get the unlocked balance of (default all subaddresses if null)
     * @return the requested unlocked balance
     */
    public ulong GetUnlockedBalance(uint? accountIdx = null, uint? subaddressIdx = null);

    /**
     * Get accounts with a given tag.
     *
     * @param tag is the tag for filtering accounts, all accounts if null
     * @return all accounts with the given tag
     */
    public List<MoneroAccount> GetAccounts(string tag);

    /**
     * Get accounts with a given tag.
     *
     * @param includeSubaddresses specifies if subaddresses should be included
     * @param tag is the tag for filtering accounts, all accounts if null
     * @return all accounts with the given tag
     */
    public List<MoneroAccount> GetAccounts(bool includeSubaddresses = false, string? tag = null);

    /**
     * Get an account.
     *
     * @param accountIdx specifies the account to Get
     * @param includeSubaddresses specifies if subaddresses should be included
     * @return the retrieved account
     */
    public MoneroAccount GetAccount(uint accountIdx, bool includeSubaddresses = false);

    /**
     * Create a new account with a label for the first subaddress.
     *
     * @param label specifies the label for account's first subaddress (optional)
     * @return the created account
     */
    public MoneroAccount CreateAccount(string? label = null);

    /**
     * Set an account label.
     *
     * @param accountIdx index of the account to set the label for
     * @param label the label to set
     */
    public void SetAccountLabel(uint accountIdx, string label);

    /**
     * Get subaddresses in an account.
     *
     * @param accountIdx specifies the account to Get subaddresses within
     * @param subaddressIndices are specific subaddresses to Get (optional)
     * @return the retrieved subaddresses
     */
    public List<MoneroSubaddress> GetSubaddresses(uint accountIdx, List<uint>? subaddressIndices = null);

    /**
     * Get a subaddress.
     *
     * @param accountIdx specifies the index of the subaddress's account
     * @param subaddressIdx specifies index of the subaddress within the account
     * @return the retrieved subaddress
     */
    public MoneroSubaddress GetSubaddress(uint accountIdx, uint subaddressIdx);

    /**
     * Create a subaddress within an account.
     *
     * @param accountIdx specifies the index of the account to Create the subaddress within
     * @param label specifies the the label for the subaddress (optional)
     * @return the created subaddress
     */
    public MoneroSubaddress CreateSubaddress(uint accountIdx, string? label = null);

    /**
     * Set a subaddress label.
     *
     * @param accountIdx index of the account to set the label for
     * @param subaddressIdx index of the subaddress to set the label for
     * @param label the label to set
     */
    public void SetSubaddressLabel(uint accountIdx, uint subaddressIdx, string label);

    /**
     * Get a wallet transaction by hash.
     *
     * @param txHash is the hash of a transaction to Get
     * @return the identified transaction or null if not found
     */
    public MoneroTxWallet? GetTx(string txHash);

    /**
     * Get all wallet transactions.  Wallet transactions contain one or more
     * transfers that are either incoming or outgoing to the wallet.
     *
     * @return all wallet transactions
     */
    public List<MoneroTxWallet> GetTxs();

    /**
     * Get wallet transactions by hash.
     *
     * @param txHashes are hashes of transactions to Get
     * @return the found transactions
     */
    public List<MoneroTxWallet> GetTxs(List<string> txHashes);

    /**
     * <p>Get wallet transactions that meet the criteria defined in a query object.</p>
     *
     * <p>Transactions must meet every criteria defined in the query in order to
     * be returned.  All criteria are optional and no filtering is applied when
     * not defined.</p>
     *
     * <p>
     * All supported query criteria:<br>
     * &nbsp;&nbsp; isConfirmed - path of the wallet to open<br>
     * &nbsp;&nbsp; password - password of the wallet to open<br>
     * &nbsp;&nbsp; networkType - network type of the wallet to open (one of MoneroNetworkType.MAINNET|TESTNET|STAGENET)<br>
     * &nbsp;&nbsp; serverUri - uri of the wallet's daemon (optional)<br>
     * &nbsp;&nbsp; serverUsername - username to authenticate with the daemon (optional)<br>
     * &nbsp;&nbsp; serverPassword - password to authenticate with the daemon (optional)<br>
     * &nbsp;&nbsp; server - MoneroRpcConnection to a monero daemon (optional)<br>
     * &nbsp;&nbsp; isConfirmed - Get txs that are confirmed or not (optional)<br>
     * &nbsp;&nbsp; inTxPool - Get txs that are in the tx pool or not (optional)<br>
     * &nbsp;&nbsp; isRelayed - Get txs that are relayed or not (optional)<br>
     * &nbsp;&nbsp; isFailed - Get txs that are failed or not (optional)<br>
     * &nbsp;&nbsp; isMinerTx - Get miner txs or not (optional)<br>
     * &nbsp;&nbsp; hash - Get a tx with the hash (optional)<br>
     * &nbsp;&nbsp; hashes - Get txs with the hashes (optional)<br>
     * &nbsp;&nbsp; paymentId - Get transactions with the payment id (optional)<br>
     * &nbsp;&nbsp; paymentIds - Get transactions with the payment ids (optional)<br>
     * &nbsp;&nbsp; hasPaymentId - Get transactions with a payment id or not (optional)<br>
     * &nbsp;&nbsp; minHeight - Get txs with height greater than or equal to the given height (optional)<br>
     * &nbsp;&nbsp; maxHeight - Get txs with height less than or equal to the given height (optional)<br>
     * &nbsp;&nbsp; isOutgoing - Get txs with an outgoing transfer or not (optional)<br>
     * &nbsp;&nbsp; isIncoming - Get txs with an incoming transfer or not (optional)<br>
     * &nbsp;&nbsp; transferQuery - Get txs that have a transfer that meets this query (optional)<br>
     * &nbsp;&nbsp; includeOutputs - specifies that tx outputs should be returned with tx results (optional)<br>
     * </p>
     *
     * @param query specifies properties of the transactions to Get
     * @return wallet transactions that meet the query
     */
    public List<MoneroTxWallet> GetTxs(MoneroTxQuery query);

    /**
     * Get all incoming and outgoing transfers to and from this wallet.  An
     * outgoing transfer represents a total amount sent from one or more
     * subaddresses within an account to individual destination addresses, each
     * with their own amount.  An incoming transfer represents a total amount
     * received into a subaddress within an account.  Transfers belong to
     * transactions which are stored on the blockchain.
     *
     * @return all wallet transfers
     */
    public List<MoneroTransfer> GetTransfers();

    /**
     * Get incoming and outgoing transfers to and from an account.  An outgoing
     * transfer represents a total amount sent from one or more subaddresses
     * within an account to individual destination addresses, each with their
     * own amount.  An incoming transfer represents a total amount received into
     * a subaddress within an account.  Transfers belong to transactions which
     * are stored on the blockchain.
     *
     * @param accountIdx is the index of the account to Get transfers from
     * @return transfers to/from the account
     */
    public List<MoneroTransfer> GetTransfers(uint accountIdx);

    /**
     * Get incoming and outgoing transfers to and from a subaddress.  An outgoing
     * transfer represents a total amount sent from one or more subaddresses
     * within an account to individual destination addresses, each with their
     * own amount.  An incoming transfer represents a total amount received into
     * a subaddress within an account.  Transfers belong to transactions which
     * are stored on the blockchain.
     *
     * @param accountIdx is the index of the account to Get transfers from
     * @param subaddressIdx is the index of the subaddress to Get transfers from
     * @return transfers to/from the subaddress
     */
    public List<MoneroTransfer> GetTransfers(uint accountIdx, uint subaddressIdx);

    /**
     * <p>Get tranfsers that meet the criteria defined in a query object.</p>
     *
     * <p>Transfers must meet every criteria defined in the query in order to be
     * returned.  All criteria are optional and no filtering is applied when not
     * defined.</p>
     *
     * All supported query criteria:<br>
     * &nbsp;&nbsp; isOutgoing - Get transfers that are outgoing or not (optional)<br>
     * &nbsp;&nbsp; isIncoming - Get transfers that are incoming or not (optional)<br>
     * &nbsp;&nbsp; address - wallet's address that a transfer either originated from (if outgoing) or is destined for (if incoming) (optional)<br>
     * &nbsp;&nbsp; accountIndex - Get transfers that either originated from (if outgoing) or are destined for (if incoming) a specific account index (optional)<br>
     * &nbsp;&nbsp; subaddressIndex - Get transfers that either originated from (if outgoing) or are destined for (if incoming) a specific subaddress index (optional)<br>
     * &nbsp;&nbsp; subaddressIndices - Get transfers that either originated from (if outgoing) or are destined for (if incoming) specific subaddress indices (optional)<br>
     * &nbsp;&nbsp; amount - amount being transferred (optional)<br>
     * &nbsp;&nbsp; destinations - individual destinations of an outgoing transfer, which is local wallet data and NOT recoverable from the blockchain (optional)<br>
     * &nbsp;&nbsp; hasDestinations - Get transfers that have destinations or not (optional)<br>
     * &nbsp;&nbsp; txQuery - Get transfers whose transaction meets this query (optional)<br>
     *
     * @param query specifies attributes of transfers to Get
     * @return wallet transfers that meet the query
     */
    public List<MoneroTransfer> GetTransfers(MoneroTransferQuery query);

    /**
     * Get all of the wallet's incoming transfers.
     *
     * @return the wallet's incoming transfers
     */
    public List<MoneroIncomingTransfer> GetIncomingTransfers();

    /**
     * <p>Get incoming transfers that meet a query.</p>
     *
     * <p>
     * All supported query criteria:<br>
     * &nbsp;&nbsp; address - Get incoming transfers to a specific address in the wallet (optional)<br>
     * &nbsp;&nbsp; accountIndex - Get incoming transfers to a specific account index (optional)<br>
     * &nbsp;&nbsp; subaddressIndex - Get incoming transfers to a specific subaddress index (optional)<br>
     * &nbsp;&nbsp; subaddressIndices - Get transfers destined for specific subaddress indices (optional)<br>
     * &nbsp;&nbsp; amount - amount being transferred (optional)<br>
     * &nbsp;&nbsp; txQuery - Get transfers whose transaction meets this query (optional)<br>
     * </p>
     *
     * @param query specifies which incoming transfers to Get
     * @return incoming transfers that meet the query
     */
    public List<MoneroIncomingTransfer> GetIncomingTransfers(MoneroTransferQuery query);

    /**
     * Get all of the wallet's outgoing transfers.
     *
     * @return the wallet's outgoing transfers
     */
    public List<MoneroOutgoingTransfer> GetOutgoingTransfers();

    /**
     * <p>Get outgoing transfers that meet a query.</p>
     *
     * <p>
     * All supported query criteria:<br>
     * &nbsp;&nbsp; address - Get outgoing transfers from a specific address in the wallet (optional)<br>
     * &nbsp;&nbsp; accountIndex - Get outgoing transfers from a specific account index (optional)<br>
     * &nbsp;&nbsp; subaddressIndex - Get outgoing transfers from a specific subaddress index (optional)<br>
     * &nbsp;&nbsp; subaddressIndices - Get outgoing transfers from specific subaddress indices (optional)<br>
     * &nbsp;&nbsp; amount - amount being transferred (optional)<br>
     * &nbsp;&nbsp; destinations - individual destinations of an outgoing transfer, which is local wallet data and NOT recoverable from the blockchain (optional)<br>
     * &nbsp;&nbsp; hasDestinations - Get transfers that have destinations or not (optional)<br>
     * &nbsp;&nbsp; txQuery - Get transfers whose transaction meets this query (optional)<br>
     * </p>
     *
     * @param query specifies which outgoing transfers to Get
     * @return outgoing transfers that meet the query
     */
    public List<MoneroOutgoingTransfer> GetOutgoingTransfers(MoneroTransferQuery query);

    /**
     * Get outputs Created from previous transactions that belong to the wallet
     * (i.e. that the wallet can spend one time).  Outputs are part of
     * transactions which are stored in blocks on the blockchain.
     *
     * @return all wallet outputs
     */
    public List<MoneroOutputWallet> GetOutputs();

    /**
     * <p>Get outputs which meet the criteria defined in a query object.</p>
     *
     * <p>Outputs must meet every criteria defined in the query in order to be
     * returned.  All criteria are optional and no filtering is applied when not
     * defined.</p>
     *
     * <p>
     * All supported query criteria:<br>
     * &nbsp;&nbsp; accountIndex - Get outputs associated with a specific account index (optional)<br>
     * &nbsp;&nbsp; subaddressIndex - Get outputs associated with a specific subaddress index (optional)<br>
     * &nbsp;&nbsp; subaddressIndices - Get outputs associated with specific subaddress indices (optional)<br>
     * &nbsp;&nbsp; amount - Get outputs with a specific amount (optional)<br>
     * &nbsp;&nbsp; minAmount - Get outputs greater than or equal to a minimum amount (optional)<br>
     * &nbsp;&nbsp; maxAmount - Get outputs less than or equal to a maximum amount (optional)<br>
     * &nbsp;&nbsp; isSpent - Get outputs that are spent or not (optional)<br>
     * &nbsp;&nbsp; keyImage - Get outputs that match the fields defined in the given key image (optional)<br>
     * &nbsp;&nbsp; txQuery - Get outputs whose transaction meets this filter (optional)<br>
     * </p>
     *
     * @param query specifies attributes of outputs to Get
     * @return the queried outputs
     */
    public List<MoneroOutputWallet> GetOutputs(MoneroOutputQuery query);

    /**
     * Export outputs in hex format.
     *
     * @param all exports all outputs if true, else exports the outputs since the last export
     * @return outputs in hex format
     */
    public string ExportOutputs(bool all = false);

    /**
     * Import outputs in hex format.
     *
     * @param outputsHex are outputs in hex format
     * @return the number of outputs imported
     */
    public int ImportOutputs(string outputsHex);

    /**
     * Export signed key images.
     *
     * @param all exports all key images if true, else exports the key images since the last export
     * @return signed key images
     */
    public List<MoneroKeyImage> ExportKeyImages(bool all = false);

    /**
     * Import signed key images and verify their spent status.
     *
     * @param keyImages are key images to import and verify (requires hex and signature)
     * @return results of the import
     */
    public MoneroKeyImageImportResult ImportKeyImages(List<MoneroKeyImage> keyImages);

    /**
     * Get new key images from the last imported outputs.
     *
     * @return the key images from the last imported outputs
     */
    public List<MoneroKeyImage> GetNewKeyImagesFromLastImport();

    /**
     * Freeze an output.
     *
     * @param keyImage key image of the output to freeze
     */
    public void FreezeOutput(string keyImage);

    /**
     * Thaw a frozen output.
     *
     * @param keyImage key image of the output to thaw
     */
    public void ThawOutput(string keyImage);

    /**
     * Check if an output is frozen.
     *
     * @param keyImage key image of the output to check if frozen
     * @return true if the output is frozen, false otherwise
     */
    public bool IsOutputFrozen(string keyImage);

    /**
     * Get the current default fee priority (unimportant, normal, elevated, etc).
     *
     * @return the current fee priority
     */
    public MoneroTxPriority GetDefaultFeePriority();

    /**
     * Create a transaction to transfer funds from this wallet.
     *
     * <p>
     * All supported configuration:<br>
     * &nbsp;&nbsp; address - single destination address (required unless `destinations` provided)<br>
     * &nbsp;&nbsp; amount - single destination amount (required unless `destinations` provided)<br>
     * &nbsp;&nbsp; accountIndex - source account index to transfer funds from (required)<br>
     * &nbsp;&nbsp; subaddressIndex - source subaddress index to transfer funds from (optional)<br>
     * &nbsp;&nbsp; subaddressIndices - source subaddress indices to transfer funds from (optional)<br>
     * &nbsp;&nbsp; relay - relay the transaction to peers to commit to the blockchain (default false)<br>
     * &nbsp;&nbsp; priority - transaction priority (default MoneroTxPriority.NORMAL)<br>
     * &nbsp;&nbsp; destinations - addresses and amounts in a multi-destination tx (required unless `address` and `amount` provided)<br>
     * &nbsp;&nbsp; subtractFeeFrom - list of destination indices to split the transaction fee (optional)<br>
     * &nbsp;&nbsp; paymentId - transaction payment ID (optional)<br>
     * &nbsp;&nbsp; unlockTime - minimum height or timestamp for the transaction to unlock (default 0)<br>
     * </p>
     *
     * @param config configures the transaction to Create
     * @return the created transaction
     */
    public MoneroTxWallet CreateTx(MoneroTxConfig config);

    /**
     * Create one or more transactions to transfer funds from this wallet.
     *
     * <p>
     * All supported configuration:<br>
     * &nbsp;&nbsp; address - single destination address (required unless `destinations` provided)<br>
     * &nbsp;&nbsp; amount - single destination amount (required unless `destinations` provided)<br>
     * &nbsp;&nbsp; accountIndex - source account index to transfer funds from (required)<br>
     * &nbsp;&nbsp; subaddressIndex - source subaddress index to transfer funds from (optional)<br>
     * &nbsp;&nbsp; subaddressIndices - source subaddress indices to transfer funds from (optional)<br>
     * &nbsp;&nbsp; relay - relay the transactions to peers to commit to the blockchain (default false)<br>
     * &nbsp;&nbsp; priority - transaction priority (default MoneroTxPriority.NORMAL)<br>
     * &nbsp;&nbsp; destinations - addresses and amounts in a multi-destination tx (required unless `address` and `amount` provided)<br>
     * &nbsp;&nbsp; paymentId - transaction payment ID (optional)<br>
     * &nbsp;&nbsp; unlockTime - minimum height or timestamp for the transactions to unlock (default 0)<br>
     * &nbsp;&nbsp; canSplit - allow funds to be transferred using multiple transactions (default true)<br>
     * </p>
     *
     * @param config configures the transactions to Create
     * @return the created transactions
     */
    public List<MoneroTxWallet> CreateTxs(MoneroTxConfig config);

    /**
     * Sweep an output with a given key image.
     *
     * <p>
     * All supported configuration:<br>
     * &nbsp;&nbsp; address - single destination address (required)<br>
     * &nbsp;&nbsp; keyImage - key image to sweep (required)<br>
     * &nbsp;&nbsp; relay - relay the transaction to peers to commit to the blockchain (default false)<br>
     * &nbsp;&nbsp; unlockTime - minimum height or timestamp for the transaction to unlock (default 0)<br>
     * &nbsp;&nbsp; priority - transaction priority (default MoneroTxPriority.NORMAL)<br>
     * </p>
     *
     * @param config configures the sweep transaction
     * @return the created transaction
     */
    public MoneroTxWallet SweepOutput(MoneroTxConfig config);

    /**
     * Sweep all unlocked funds according to the given config.
     *
     * <p>
     * All supported configuration:<br>
     * &nbsp;&nbsp; address - single destination address (required)<br>
     * &nbsp;&nbsp; accountIndex - source account index to sweep from (optional, defaults to all accounts)<br>
     * &nbsp;&nbsp; subaddressIndex - source subaddress index to sweep from (optional, defaults to all subaddresses)<br>
     * &nbsp;&nbsp; subaddressIndices - source subaddress indices to sweep from (optional)<br>
     * &nbsp;&nbsp; relay - relay the transactions to peers to commit to the blockchain (default false)<br>
     * &nbsp;&nbsp; priority - transaction priority (default MoneroTxPriority.NORMAL)<br>
     * &nbsp;&nbsp; unlockTime - minimum height or timestamp for the transactions to unlock (default 0)<br>
     * &nbsp;&nbsp; sweepEachSubaddress - sweep each subaddress individually if true (default false)<br>
     * </p>
     *
     * @param config is the sweep configuration
     * @return the created transactions
     */
    public List<MoneroTxWallet> SweepUnlocked(MoneroTxConfig config);

    /**
     * Sweep all unmixable dust outputs back to the wallet to make them easier to spend and mix.
     *
     * NOTE: Dust only exists pre RCT, so this method will throw "no dust to sweep" on new wallets.
     *
     * @param relay specifies if the resulting transaction should be relayed (defaults to false i.e. not relayed)
     * @return the created transactions
     */
    public List<MoneroTxWallet> SweepDust(bool relay);

    /**
     * Relay a previously Created transaction.
     *
     * @param txMetadata is transaction metadata previously Created without relaying
     * @return the hash of the relayed tx
     */
    public string RelayTx(string txMetadata);

    /**
     * Relay a previously Created transaction.
     *
     * @param tx is the transaction to relay
     * @return the hash of the relayed tx
     */
    public string RelayTx(MoneroTxWallet tx);

    /**
     * Relay previously Created transactions.
     *
     * @param txMetadatas are transaction metadata previously Created without relaying
     * @return the hashes of the relayed txs
     */
    public List<string> RelayTxs(List<string> txMetadatas);

    /**
     * Relay previously Created transactions.
     *
     * @param txs are the transactions to relay
     * @return the hashes of the relayed txs
     */
    public List<string> RelayTxs(List<MoneroTxWallet> txs);

    /**
     * Describe a tx set from unsigned tx hex.
     *
     * @param unsignedTxHex unsigned tx hex
     * @return the tx set containing structured transactions
     */
    public MoneroTxSet DescribeUnsignedTxSet(string unsignedTxHex);

    /**
     * Describe a tx set from multisig tx hex.
     *
     * @param multisigTxHex multisig tx hex
     * @return the tx set containing structured transactions
     */
    public MoneroTxSet DescribeMultisigTxSet(string multisigTxHex);

    /**
     * Describe a tx set containing unsigned or multisig tx hex to a new tx set containing structured transactions.
     *
     * @param txSet is a tx set containing unsigned or multisig tx hex
     * @return the tx set containing structured transactions
     */
    public MoneroTxSet DescribeTxSet(MoneroTxSet txSet);

    /**
     * Sign unsigned transactions from a view-only wallet.
     *
     * @param unsignedTxHex is unsigned transaction hex from when the transactions were Created
     * @return the signed transaction set
     */
    public MoneroTxSet SignTxs(string unsignedTxHex);

    /**
     * Submit signed transactions from a view-only wallet.
     *
     * @param signedTxHex is signed transaction hex from signTxs()
     * @return the resulting transaction hashes
     */
    public List<string> SubmitTxs(string signedTxHex);

    /**
     * Sign a message.
     *
     * @param message the message to sign
     * @param signatureType sign with spend key or view key
     * @param accountIdx the account index of the message signature (default 0)
     * @param subaddressIdx the subaddress index of the message signature (default 0)
     * @return the signature
     */
    public string SignMessage(string message, MoneroMessageSignatureType signatureType = MoneroMessageSignatureType.SIGN_WITH_SPEND_KEY, uint accountIdx = 0, uint subaddressIdx = 0);

    /**
     * Verify a signature on a message.
     *
     * @param message is the signed message
     * @param address is the signing address
     * @param signature is the signature
     * @return the message signature verification result
     */
    public MoneroMessageSignatureResult VerifyMessage(string message, string address, string signature);

    /**
     * Get a transaction's secret key from its hash.
     *
     * @param txHash is the transaction's hash
     * @return is the transaction's secret key
     */
    public string GetTxKey(string txHash);

    /**
     * Check a transaction in the blockchain with its secret key.
     *
     * @param txHash specifies the transaction to check
     * @param txKey is the transaction's secret key
     * @param address is the destination public address of the transaction
     * @return the result of the check
     */
    public MoneroCheckTx CheckTxKey(string txHash, string txKey, string address);

    /**
     * Get a transaction signature to prove it.
     *
     * @param txHash specifies the transaction to prove
     * @param address is the destination public address of the transaction
     * @param message is a message to include with the signature to further authenticate the proof (optional)
     * @return the transaction signature
     */
    public string GetTxProof(string txHash, string address, string? message = null);

    /**
     * Prove a transaction by checking its signature.
     *
     * @param txHash specifies the transaction to prove
     * @param address is the destination public address of the transaction
     * @param message is a message included with the signature to further authenticate the proof (optional)
     * @param signature is the transaction signature to confirm
     * @return the result of the check
     */
    public MoneroCheckTx CheckTxProof(string txHash, string address, string message, string signature);

    /**
     * Generate a signature to prove a spend. Unlike proving a transaction, it does not require the destination public address.
     *
     * @param txHash specifies the transaction to prove
     * @param message is a message to include with the signature to further authenticate the proof (optional)
     * @return the transaction signature
     */
    public string GetSpendProof(string txHash, string? message = null);

    /**
     * Prove a spend using a signature. Unlike proving a transaction, it does not require the destination public address.
     *
     * @param txHash specifies the transaction to prove
     * @param message is a message included with the signature to further authenticate the proof (optional)
     * @param signature is the transaction signature to confirm
     * @return true if the signature is good, false otherwise
     */
    public bool CheckSpendProof(string txHash, string message, string signature);

    /**
     * Generate a signature to prove the entire balance of the wallet.
     *
     * @param message is a message included with the signature to further authenticate the proof (optional)
     * @return the reserve proof signature
     */
    public string GetReserveProofWallet(string message);

    /**
     * Generate a signature to prove an available amount in an account.
     *
     * @param accountIdx specifies the account to prove ownership of the amount
     * @param amount is the minimum amount to prove as available in the account
     * @param message is a message to include with the signature to further authenticate the proof (optional)
     * @return the reserve proof signature
     */
    public string GetReserveProofAccount(uint accountIdx, ulong amount, string message);

    /**
     * Proves a wallet has a disposable reserve using a signature.
     *
     * @param address is the public wallet address
     * @param message is a message included with the signature to further authenticate the proof (optional)
     * @param signature is the reserve proof signature to check
     * @return the result of checking the signature proof
     */
    public MoneroCheckReserve CheckReserveProof(string address, string message, string signature);

    /**
     * Get a transaction note.
     *
     * @param txHash specifies the transaction to Get the note of
     * @return the tx note
     */
    public string? GetTxNote(string txHash);

    /**
     * Get notes for multiple transactions.
     *
     * @param txHashes identify the transactions to Get notes for
     * @return notes for the transactions
     */
    public List<string> GetTxNotes(List<string> txHashes);

    /**
     * Set a note for a specific transaction.
     *
     * @param txHash specifies the transaction
     * @param note specifies the note
     */
    public void SetTxNote(string txHash, string note);

    /**
     * Set notes for multiple transactions.
     *
     * @param txHashes specify the transactions to set notes for
     * @param notes are the notes to set for the transactions
     */
    public void SetTxNotes(List<string> txHashes, List<string> notes);

    /**
     * Get all address book entries.
     *
     * @return the address book entries
     */
    public List<MoneroAddressBookEntry> GetAddressBookEntries();

    /**
     * Get address book entries.
     *
     * @param entryIndices are indices of the entries to Get (optional)
     * @return the address book entries
     */
    public List<MoneroAddressBookEntry> GetAddressBookEntries(List<uint> entryIndices);

    /**
     * Add an address book entry.
     *
     * @param address is the entry address
     * @param description is the entry description (optional)
     * @return the index of the added entry
     */
    public int AddAddressBookEntry(string address, string description);

    /**
     * Edit an address book entry.
     *
     * @param index is the index of the address book entry to edit
     * @param setAddress specifies if the address should be updated
     * @param address is the updated address
     * @param setDescription specifies if the description should be updated
     * @param description is the updated description
     */
    public void EditAddressBookEntry(uint index, bool setAddress, string address, bool setDescription, string description);

    /**
     * Delete an address book entry.
     *
     * @param entryIdx is the index of the entry to delete
     */
    public void DeleteAddressBookEntry(uint entryIdx);

    /**
     * Tag accounts.
     *
     * @param tag is the tag to apply to the specified accounts
     * @param accountIndices are the indices of the accounts to tag
     */
    public void TagAccounts(string tag, List<uint> accountIndices);

    /**
     * Untag acconts.
     *
     * @param accountIndices are the indices of the accounts to untag
     */
    public void UntagAccounts(List<uint> accountIndices);

    /**
     * Return all account tags.
     *
     * @return the wallet's account tags
     */
    public List<MoneroAccountTag> GetAccountTags();

    /**
     * Sets a human-readable description for a tag.
     *
     * @param tag is the tag to set a description for
     * @param label is the label to set for the tag
     */
    public void SetAccountTagLabel(string tag, string label);

    /**
     * Creates a payment URI from a send configuration.
     *
     * @param config specifies configuration for a potential tx
     * @return the payment uri
     */
    public string GetPaymentUri(MoneroTxConfig config);

    /**
     * Parses a payment URI to a transaction configuration.
     *
     * @param uri is the payment uri to parse
     * @return the send configuration parsed from the uri
     */
    public MoneroTxConfig ParsePaymentUri(string uri);

    /**
     * Get an attribute.
     *
     * @param key is the attribute to Get the value of
     * @return the attribute's value
     */
    public string? GetAttribute(string key);

    /**
     * Set an arbitrary attribute.
     *
     * @param key is the attribute key
     * @param val is the attribute value
     */
    public void SetAttribute(string key, string val);

    /**
     * Start mining.
     *
     * @param numThreads is the number of threads Created for mining (optional)
     * @param backgroundMining specifies if mining should occur in the background (optional)
     * @param ignoreBattery specifies if the battery should be ignored for mining (optional)
     */
    public void StartMining(ulong numThreads, bool backgroundMining, bool ignoreBattery);

    /**
     * Stop mining.
     */
    public void StopMining();

    /**
     * Indicates if importing multisig data is needed for returning a correct balance.
     *
     * @return true if importing multisig data is needed for returning a correct balance, false otherwise
     */
    public bool IsMultisigImportNeeded();

    /**
     * Indicates if this wallet is a multisig wallet.
     *
     * @return true if this is a multisig wallet, false otherwise
     */
    public bool IsMultisig();

    /**
     * Get multisig info about this wallet.
     *
     * @return multisig info about this wallet
     */
    public MoneroMultisigInfo GetMultisigInfo();

    /**
     * Get multisig info as hex to share with participants to begin creating a
     * multisig wallet.
     *
     * @return this wallet's multisig hex to share with participants
     */
    public string PrepareMultisig();

    /**
     * Make this wallet multisig by importing multisig hex from participants.
     *
     * @param multisigHexes are multisig hex from each participant
     * @param threshold is the number of signatures needed to sign transfers
     * @param password is the wallet password
     * @return this wallet's multisig hex to share with participants
     */
    public string MakeMultisig(List<string> multisigHexes, int threshold, string password);

    /**
     * Exchange multisig hex with participants in a M/N multisig wallet.
     *
     * This process must be repeated with participants exactly N-M times.
     *
     * @param multisigHexes are multisig hex from each participant
     * @param password is the wallet's password // TODO monero-project: redundant? wallet is Created with password
     * @return the result which has the multisig's address xor this wallet's multisig hex to share with participants iff not done
     */
    public MoneroMultisigInitResult ExchangeMultisigKeys(List<string> multisigHexes, string password);

    /**
     * Export this wallet's multisig info as hex for other participants.
     *
     * @return this wallet's multisig info as hex for other participants
     */
    public string ExportMultisigHex();

    /**
     * Import multisig info as hex from other participants.
     *
     * @param multisigHexes are multisig hex from each participant
     * @return the number of outputs signed with the given multisig hex
     */
    public int ImportMultisigHex(List<string> multisigHexes);

    /**
     * Sign multisig transactions from a multisig wallet.
     *
     * @param multisigTxHex represents unsigned multisig transactions as hex
     * @return the result of signing the multisig transactions
     */
    public MoneroMultisigSignResult SignMultisigTxHex(string multisigTxHex);

    /**
     * Submit signed multisig transactions from a multisig wallet.
     *
     * @param signedMultisigTxHex is signed multisig hex returned from signMultisigTxHex()
     * @return the resulting transaction hashes
     */
    public List<string> SubmitMultisigTxHex(string signedMultisigTxHex);

    /**
     * Change the wallet password.
     *
     * @param oldPassword is the wallet's old password
     * @param newPassword is the wallet's new password
     */
    public void ChangePassword(string oldPassword, string newPassword);

    /**
     * Save the wallet at its current path.
     */
    public void Save();

    /**
     * Optionally save then close the wallet.
     *
     * @param save specifies if the wallet should be saved before being closed (default false)
     */
    public void Close(bool save = false);

    /**
     * Indicates if this wallet is closed or not.
     *
     * @return true if the wallet is closed, false otherwise
     */
    public bool IsClosed();
}