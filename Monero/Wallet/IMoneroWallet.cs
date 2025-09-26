using Monero.Common;
using Monero.Wallet.Common;

namespace Monero.Wallet;

public interface IMoneroWallet
{
    static readonly string DefaultLanguage = "English";

    /**
    * Register a listener to receive wallet notifications.
    *
    * @param listener is the listener to receive wallet notifications
    */
    void AddListener(MoneroWalletListener listener);

    /**
     * Unregister a listener to receive wallet notifications.
     *
     * @param listener is the listener to unregister
     */
    void RemoveListener(MoneroWalletListener listener);

    /**
     * Get the listeners registered with the wallet.
     *
     * @return the registered listeners
     */
    List<MoneroWalletListener> GetListeners();

    /**
     * Indicates if the wallet is view-only, meaning it does not have the private
     * spent key and can therefore only observe incoming outputs.
     *
     * @return {bool} true if the wallet is view-only, false otherwise
     */
    Task<bool> IsViewOnly();

    /**
     * Sets the wallet's connection to a daemon.
     *
     * @param connection The daemon RPC connection details (e.g., URI, credentials).
     * @param isTrusted  Indicates whether the daemon is trusted (optional).
     * @param sslOptions SSL options for secure connection (optional).
     */
    Task SetDaemonConnection(MoneroRpcConnection? connection, bool? isTrusted, SslOptions? sslOptions);

    /**
     * Get the wallet's daemon connection.
     *
     * @return the wallet's daemon connection
     */
    Task<MoneroRpcConnection?> GetDaemonConnection();

    /**
     * Set the Tor proxy to the daemon.
     *
     * @param uri the Tor proxy URI
     */
    Task SetProxyUri(string? uri);

    /**
     * Indicates if the wallet is connected a daemon.
     *
     * @return true if the wallet is connected to a daemon, false otherwise
     */
    Task<bool> IsConnectedToDaemon();

    /**
     * Returns the wallet version.
     *
     * @return the wallet version
     */
    Task<MoneroVersion> GetVersion();

    Task<MoneroNetworkType> GetNetworkType();

    /**
     * Get the wallet's path.
     *
     * @return the path the wallet can be opened with
     */
    Task<string> GetPath();

    /**
     * Get the wallet's mnemonic phrase or seed.
     *
     * @return the wallet's mnemonic phrase or seed.
     */
    Task<string> GetSeed();

    /**
     * Get the language of the wallet's mnemonic phrase or seed.
     *
     * @return the language of the wallet's mnemonic phrase or seed
     */
    Task<string> GetSeedLanguage();

    /**
     * Get the wallet's private view key.
     *
     * @return the wallet's private view key
     */
    Task<string> GetPrivateViewKey();

    /**
     * Get the wallet's private spend key.
     *
     * @return the wallet's private spend key
     */
    Task<string> GetPrivateSpendKey();

    /**
     * Get the wallet's public view key.
     *
     * @return the wallet's public view key
     */
    Task<string> GetPublicViewKey();

    /**
     * Get the wallet's public spend key.
     *
     * @return the wallet's public spend key
     */
    Task<string> GetPublicSpendKey();

    /**
     * Get the wallet's primary address.
     *
     * @return the wallet's primary address
     */
    Task<string> GetPrimaryAddress();

    /**
     * Get the address of a specific subaddress.
     *
     * @param accountIdx specifies the account index of the address's subaddress
     * @param subaddressIdx specifies the subaddress index within the account
     * @return the receive address of the specified subaddress
     */
    Task<string?> GetAddress(uint accountIdx, uint subaddressIdx);

    /**
     * Get the account and subaddress index of the given address.
     *
     * @param address is the address to Get the account and subaddress index from
     * @return the account and subaddress indices
     */
    Task<MoneroSubaddress> GetAddressIndex(string address);

    /**
     * Get an integrated address based on the given standard address and payment
     * ID. Uses the wallet's primary address if an address is not given.
     * Generates a random payment ID if a payment ID is not given.
     *
     * @param standardAddress is the standard address to generate the integrated address from (wallet's primary address if null)
     * @param paymentId is the payment ID to generate an integrated address from (randomly generated if null)
     * @return the integrated address
     */
    Task<MoneroIntegratedAddress> GetIntegratedAddress(string? standardAddress, string? paymentId);

    /**
     * Decode an integrated address to Get its standard address and payment id.
     *
     * @param integratedAddress is an integrated address to decode
     * @return the decoded integrated address including standard address and payment id
     */
    Task<MoneroIntegratedAddress> DecodeIntegratedAddress(string integratedAddress);

    /**
     * Get the block height that the wallet is Synced to.
     *
     * @return the block height that the wallet is Synced to
     */
    Task<ulong> GetHeight();

    /**
     * Get the blockchain's height.
     *
     * @return the blockchain's height
     */
    Task<ulong> GetDaemonHeight();

    /**
     * Get the blockchain's height by date as a conservative estimate for scanning.
     *
     * @param year year of the height to Get
     * @param month month of the height to Get as a number between 1 and 12
     * @param day day of the height to Get as a number between 1 and 31
     * @return the blockchain's approximate height at the given date
     */
    Task<ulong> GetHeightByDate(int year, int month, int day);

    /**
     * Start background Synchronizing with a maximum period between Syncs.
     *
     * @param SyncPeriodInMs maximum period between Syncs in milliseconds
     */
    Task StartSyncing(ulong? syncPeriodInMs);

    /**
     * Stop Synchronizing the wallet with the daemon.
     */
    Task StopSyncing();

    /**
     * Scan transactions by their hash/id.
     *
     * @param txHashes tx hashes to scan
     */
    Task ScanTxs(List<string> txHashes);

    /**
     * Rescan the blockchain for spent outputs.
     *
     * Note: this can only be called with a trusted daemon.
     *
     * Example use case: peer multisig hex is import when connected to an untrusted daemon,
     * so the wallet will not rescan spent outputs.  Then the wallet connects to a trusted
     * daemon.  This method should be manually invoked to rescan outputs.
     */
    Task RescanSpent();

    /**
     * Rescan the blockchain from scratch, losing any information which cannot be recovered from
     * the blockchain itself.
     *
     * WARNING: This method discards local wallet data like destination addresses, tx secret keys,
     * tx notes, etc.
     */
    Task RescanBlockchain();

    /**
     * Get a subaddress's balance.
     *
     * @param accountIdx index of the account to Get the balance of (default all accounts if null)
     * @param subaddressIdx index of the subaddress to Get the balance of (default all subaddresses if null)
     * @return the requested balance
     */
    Task<ulong> GetBalance(uint? accountIdx, uint? subaddressIdx);

    /**
     * Get accounts with a given tag.
     *
     * @param includeSubaddresses specifies if subaddresses should be included
     * @param skipBalances skips balances if true
     * @param tag is the tag for filtering accounts, all accounts if null
     * @return all accounts with the given tag
     */
    Task<List<MoneroAccount>> GetAccounts(bool includeSubaddresses, bool skipBalances, string? tag);

    /**
     * Create a new account with a label for the first subaddress.
     *
     * @param label specifies the label for the account's first subaddress (optional)
     * @return the created account
     */
    Task<MoneroAccount> CreateAccount(string? label);

    /**
     * Set an account label.
     *
     * @param accountIdx Index of the account to set the label for
     * @param subaddressIdx Index of the subaddress to set the label for
     * @param label The label to set
     */
    Task SetAccountLabel(uint accountIdx, uint subaddressIdx, string label);

    /**
     * Get subaddresses in an account.
     *
     * @param accountIdx specifies the account to Get subaddresses within
     * @param skipBalances skip balances if true, else get balances
     * @param subaddressIndices are specific subaddresses to Get (optional)
     * @return the retrieved subaddresses
     */
    Task<List<MoneroSubaddress>> GetSubaddresses(uint accountIdx, bool skipBalances, List<uint>? subaddressIndices);

    /**
     * Get a subaddress.
     *
     * @param accountIdx specifies the index of the subaddress's account
     * @param subaddressIdx specifies index of the subaddress within the account
     * @return the retrieved subaddress
     */
    Task<MoneroSubaddress> GetSubaddress(uint accountIdx, uint subaddressIdx);

    /**
     * Create a subaddress within an account.
     *
     * @param accountIdx specifies the index of the account to Create the subaddress within
     * @param label specifies the label for the subaddress (optional)
     * @return the created subaddress
     */
    Task<MoneroSubaddress> CreateSubaddress(uint accountIdx, string? label);

    /**
     * Gets transfers that meet the criteria defined in a query object.
     * <p>
     *     Transfers must meet <strong>every</strong> criterion defined in the query in order to be returned.
     *     All criteria are optional — if not defined, no filtering is applied.
     * </p>
     * <p>
     *     <strong>Supported query criteria:</strong>
     * </p>
     * <ul>
     *     <li><code>isOutgoing</code> – Get transfers that are outgoing or not (optional)</li>
     *     <li><code>isIncoming</code> – Get transfers that are incoming or not (optional)</li>
     *     <li>
     *         <code>address</code> – Wallet address that a transfer either originated from (if outgoing) or is destined for
     *         (if incoming) (optional)
     *     </li>
     *     <li>
     *         <code>accountIndex</code> – Transfers that either originated from or are destined for a specific account index
     *         (optional)
     *     </li>
     *     <li>
     *         <code>subaddressIndex</code> – Transfers that either originated from or are destined for a specific subaddress
     *         index (optional)
     *     </li>
     *     <li>
     *         <code>subaddressIndices</code> – Transfers that either originated from or are destined for specific subaddress
     *         indices (optional)
     *     </li>
     *     <li><code>amount</code> – Amount being transferred (optional)</li>
     *     <li>
     *         <code>destinations</code> – Individual destinations of an outgoing transfer (local wallet data, not recoverable
     *         from the blockchain) (optional)
     *     </li>
     *     <li><code>hasDestinations</code> – Get transfers that have destinations or not (optional)</li>
     *     <li><code>txQuery</code> – Get transfers whose transaction meets this query (optional)</li>
     * </ul>
     * @param query specifies attributes of transfers to get
     * @return list of wallet transfers that meet the query
     */
    Task<List<MoneroTransfer>> GetTransfers(MoneroTransferQuery query);


    /**
     * Export outputs in hex format.
     *
     * @param all exports all outputs if true, else exports the outputs since the last export
     * @return outputs in hex format
     */
    Task<string> ExportOutputs(bool all);

    /**
     * Import outputs in hex format.
     *
     * @param outputsHex are outputs in hex format
     * @return the number of outputs imported
     */
    Task<int> ImportOutputs(string outputsHex);

    /**
     * Export signed key images.
     *
     * @param all exports all key images if true, else exports the key images since the last export
     * @return signed key images
     */
    Task<List<MoneroKeyImage>> ExportKeyImages(bool all);

    /**
     * Import signed key images and verify their spent status.
     *
     * @param keyImages are key images to import and verify (requires hex and signature)
     * @return results of the import
     */
    Task<MoneroKeyImageImportResult> ImportKeyImages(List<MoneroKeyImage> keyImages);

    /**
     * Freeze an output.
     *
     * @param keyImage key image of the output to freeze
     */
    Task FreezeOutput(string keyImage);

    /**
     * Thaw a frozen output.
     *
     * @param keyImage key image of the output to thaw
     */
    Task ThawOutput(string keyImage);

    /**
     * Check if an output is frozen.
     *
     * @param keyImage key image of the output to check if frozen
     * @return true if the output is frozen, false otherwise
     */
    Task<bool> IsOutputFrozen(string keyImage);

    /**
     * Get the current default fee priority (unimportant, normal, elevated, etc).
     *
     * @return the current fee priority
     */
    Task<MoneroTxPriority> GetDefaultFeePriority();

    /**
     * Relay a previously Created transaction.
     *
     * @param txMetadata is transaction metadata previously Created without relaying
     * @return the hash of the relayed tx
     */
    Task<string> RelayTx(string txMetadata);

    /**
     * Submit signed transactions from a view-only wallet.
     *
     * @param signedTxHex is signed transaction hex from signTxs()
     * @return the resulting transaction hashes
     */
    Task<List<string>> SubmitTxs(string signedTxHex);

    /**
     * Sign a message.
     *
     * @param message the message to sign
     * @param signatureType sign with spent key or view key
     * @param accountIdx the account index of the message signature (default 0)
     * @param subaddressIdx the subaddress index of the message signature (default 0)
     * @return the signature
     */
    Task<string> SignMessage(string message, MoneroMessageSignatureType signatureType, uint accountIdx,
        uint subaddressIdx);

    /**
     * Verify a signature on a message.
     *
     * @param message is the signed message
     * @param address is the signing address
     * @param signature is the signature
     * @return the message signature verification result
     */
    Task<MoneroMessageSignatureResult> VerifyMessage(string message, string address, string signature);

    /**
     * Get a transaction's secret key from its hash.
     *
     * @param txHash is the transaction's hash
     * @return is the transaction's secret key
     */
    Task<string> GetTxKey(string txHash);

    /**
     * Check a transaction in the blockchain with its secret key.
     *
     * @param txHash specifies the transaction to check
     * @param txKey is the transaction's secret key
     * @param address is the destination public address of the transaction
     * @return the result of the check
     */
    Task<MoneroCheckTx> CheckTxKey(string txHash, string txKey, string address);

    /**
     * Get a transaction signature to prove it.
     *
     * @param txHash specifies the transaction to prove
     * @param address is the destination public address of the transaction
     * @param message is a message to include with the signature to further authenticate the proof (optional)
     * @return the transaction signature
     */
    Task<string> GetTxProof(string txHash, string address, string? message);

    /**
     * Prove a transaction by checking its signature.
     *
     * @param txHash specifies the transaction to prove
     * @param address is the destination public address of the transaction
     * @param message is a message included with the signature to further authenticate the proof (optional)
     * @param signature is the transaction signature to confirm
     * @return the result of the check
     */
    Task<MoneroCheckTx> CheckTxProof(string txHash, string address, string message, string signature);

    /**
     * Generate a signature to prove a spend. Unlike proving a transaction, it does not require the destination public address.
     *
     * @param txHash specifies the transaction to prove
     * @param message is a message to include with the signature to further authenticate the proof (optional)
     * @return the transaction signature
     */
    Task<string> GetSpendProof(string txHash, string? message);

    /**
     * Prove a spend using a signature. Unlike proving a transaction, it does not require the destination public address.
     *
     * @param txHash specifies the transaction to prove
     * @param message is a message included with the signature to further authenticate the proof (optional)
     * @param signature is the transaction signature to confirm
     * @return true if the signature is good, false otherwise
     */
    Task<bool> CheckSpendProof(string txHash, string message, string signature);

    /**
     * Generate a signature to prove the entire balance of the wallet.
     *
     * @param message is a message included with the signature to further authenticate the proof (optional)
     * @return the reserve proof signature
     */
    Task<string> GetReserveProofWallet(string message);

    /**
     * Generate a signature to prove an available amount in an account.
     *
     * @param accountIdx specifies the account to prove ownership of the amount
     * @param amount is the minimum amount to prove as available in the account
     * @param message is a message to include with the signature to further authenticate the proof (optional)
     * @return the reserve proof signature
     */
    Task<string> GetReserveProofAccount(uint accountIdx, ulong amount, string message);

    /**
     * Proves a wallet has a disposable reserve using a signature.
     *
     * @param address is the public wallet address
     * @param message is a message included with the signature to further authenticate the proof (optional)
     * @param signature is the reserve proof signature to check
     * @return the result of checking the signature proof
     */
    Task<MoneroCheckReserve> CheckReserveProof(string address, string message, string signature);

    /**
     * Get notes for multiple transactions.
     *
     * @param txHashes identify the transactions to Get notes for
     * @return notes for the transactions
     */
    Task<List<string>> GetTxNotes(List<string> txHashes);

    /**
     * Set notes for multiple transactions.
     *
     * @param txHashes specify the transactions to set notes for
     * @param notes are the notes to set for the transactions
     */
    Task SetTxNotes(List<string> txHashes, List<string> notes);

    /**
     * Get address book entries.
     *
     * @param entryIndices are indices of the entries to Get (optional)
     * @return the address book entries
     */
    Task<List<MoneroAddressBookEntry>> GetAddressBookEntries(List<uint> entryIndices);

    /**
     * Add an address book entry.
     *
     * @param address is the entry address
     * @param description is the entry description (optional)
     * @return the index of the added entry
     */
    Task<int> AddAddressBookEntry(string address, string description);

    /**
     * Edit an address book entry.
     *
     * @param index is the index of the address book entry to edit
     * @param setAddress specifies if the address should be updated
     * @param address is the updated address
     * @param setDescription specifies if the description should be updated
     * @param description is the updated description
     */
    Task EditAddressBookEntry(uint index, bool setAddress, string address, bool setDescription,
        string description);

    /**
     * Delete an address book entry.
     *
     * @param entryIdx is the index of the entry to delete
     */
    Task DeleteAddressBookEntry(uint entryIdx);

    /**
     * Tag accounts.
     *
     * @param tag is the tag to apply to the specified accounts
     * @param accountIndices are the indices of the accounts to tag
     */
    Task TagAccounts(string tag, List<uint> accountIndices);

    /**
     * Untag acconts.
     *
     * @param accountIndices are the indices of the accounts to untag
     */
    Task UntagAccounts(List<uint> accountIndices);

    /**
     * Return all account tags.
     *
     * @return the wallet's account tags
     */
    Task<List<MoneroAccountTag>> GetAccountTags();

    /**
     * Sets a human-readable description for a tag.
     *
     * @param tag is the tag to set a description for
     * @param label is the label to set for the tag
     */
    Task SetAccountTagLabel(string tag, string label);

    /**
     * Creates a payment URI from a send configuration.
     *
     * @param config specifies configuration for a potential tx
     * @return the payment uri
     */
    Task<string> GetPaymentUri(MoneroTxConfig config);

    /**
     * Parses a payment URI to a transaction configuration.
     *
     * @param uri is the payment uri to parse
     * @return the send configuration parsed from the uri
     */
    Task<MoneroTxConfig> ParsePaymentUri(string uri);

    /**
     * Get an attribute.
     *
     * @param key is the attribute to Get the value of
     * @return the attribute's value
     */
    Task<string?> GetAttribute(string key);

    /**
     * Set an arbitrary attribute.
     *
     * @param key is the attribute key
     * @param val is the attribute value
     */
    Task SetAttribute(string key, string val);

    /**
     * Start mining.
     *
     * @param numThreads is the number of threads Created for mining (optional)
     * @param backgroundMining specifies if mining should occur in the background (optional)
     * @param ignoreBattery specifies if the battery should be ignored for mining (optional)
     */
    Task StartMining(ulong numThreads, bool backgroundMining, bool ignoreBattery);

    /**
     * Stop mining.
     */
    Task StopMining();

    /**
     * Indicates if importing multisig data is needed for returning a correct balance.
     *
     * @return true if importing multisig data is needed for returning a correct balance, false otherwise
     */
    Task<bool> IsMultisigImportNeeded();

    /**
     * Indicates if this wallet is a multisig wallet.
     *
     * @return true if this is a multisig wallet, false otherwise
     */
    Task<bool> IsMultisig();

    /**
     * Get multisig info about this wallet.
     *
     * @return multisig info about this wallet
     */
    Task<MoneroMultisigInfo> GetMultisigInfo();

    /**
     * Get multisig info as hex to share with participants to begin creating a
     * multisig wallet.
     *
     * @return this wallet's multisig hex to share with participants
     */
    Task<string> PrepareMultisig();

    /**
     * Make this wallet multisig by importing multisig hex from participants.
     *
     * @param multisigHexes are multisig hex from each participant
     * @param threshold is the number of signatures needed to sign transfers
     * @param password is the wallet password
     * @return this wallet's multisig hex to share with participants
     */
    Task<string> MakeMultisig(List<string> multisigHexes, int threshold, string password);

    /**
     * Exchange multisig hex with participants in a M/N multisig wallet.
     *
     * This process must be repeated with participants exactly N-M times.
     *
     * @param multisigHexes are multisig hex from each participant
     * @param password is the wallet's password // TODO monero-project: redundant? wallet is Created with password
     * @return the result which has the multisig's address xor this wallet's multisig hex to share with participants iff not done
     */
    Task<MoneroMultisigInitResult> ExchangeMultisigKeys(List<string> multisigHexes, string password);

    /**
     * Export this wallet's multisig info as hex for other participants.
     *
     * @return this wallet's multisig info as hex for other participants
     */
    Task<string> ExportMultisigHex();

    /**
     * Import multisig info as hex from other participants.
     *
     * @param multisigHexes are multisig hex from each participant
     * @return the number of outputs signed with the given multisig hex
     */
    Task<int> ImportMultisigHex(List<string> multisigHexes);

    /**
     * Sign multisig transactions from a multisig wallet.
     *
     * @param multisigTxHex represents unsigned multisig transactions as hex
     * @return the result of signing the multisig transactions
     */
    Task<MoneroMultisigSignResult> SignMultisigTxHex(string multisigTxHex);

    /**
     * Submit signed multisig transactions from a multisig wallet.
     *
     * @param signedMultisigTxHex is signed multisig hex returned from signMultisigTxHex()
     * @return the resulting transaction hashes
     */
    Task<List<string>> SubmitMultisigTxHex(string signedMultisigTxHex);

    /**
     * Change the wallet password.
     *
     * @param oldPassword is the wallet's old password
     * @param newPassword is the wallet's new password
     */
    Task ChangePassword(string oldPassword, string newPassword);

    /**
     * Save the wallet at its current path.
     */
    Task Save();

    /**
     * Optionally save then close the wallet.
     *
     * @param save specifies if the wallet should be saved before being closed (default false)
     */
    Task Close(bool save);

    /**
     * Indicates if this wallet is closed or not.
     *
     * @return true if the wallet is closed, false otherwise
     */
    Task<bool> IsClosed();
}