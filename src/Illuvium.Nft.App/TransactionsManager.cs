using Illuvium.Nft.App.Models;
using Newtonsoft.Json.Schema.Generation;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Linq;
using Illuvium.Nft.App.Repository;

namespace Illuvium.Nft.App
{
    public class TransactionsManager : ITransactionsManager
    {
        /// <summary>
        /// Allowed transaction operations.
        /// </summary>
        private const string Mint = "Mint";
        private const string Burn = "Burn";
        private const string Transfer = "Transfer";

        private readonly IFileSystem _fileSystemWorker;

        private TokenStorage _tokenStorage;

        public TransactionsManager(IFileSystem fileSystemWorker)
        {
            _fileSystemWorker = fileSystemWorker;
            _tokenStorage = new TokenStorage();
        }

        #region Public members

        /// <summary>
        /// Tries to recover state from previous run.
        /// </summary>
        public async Task<bool> InitializeAsync()
        {
            bool result = true;

            try
            {
                _tokenStorage = await _fileSystemWorker.ReadAsync();
            }
            catch
            {
                result = false;
            }

            if (_tokenStorage == null)
                _tokenStorage = new TokenStorage();

            return result;
        }

        /// <summary>
        /// Reset the state and save the clean state.
        /// </summary>
        public async Task<bool> ClearAsync()
        {
            _tokenStorage = new TokenStorage();

            return await _fileSystemWorker.SaveAsync(_tokenStorage);

            // return await _fileSystemWorker.DeleteAsync(); // I decided not to delete the file, but instead, to rewrite the one.
        }

        /// <summary>
        /// Save on disk.
        /// </summary>
        public async Task<bool> PersistAsync()
        {
            return await _fileSystemWorker.SaveAsync(_tokenStorage);
        }

        /// <summary>
        /// Finds list of tokens owned by wallet.
        /// </summary>
        public async Task<List<string>> GetTokensAsync(string walletId)
        {
            var result = new List<string>();

            if (_tokenStorage.WalletNftTokensMap.ContainsKey(walletId) &&
                _tokenStorage.WalletNftTokensMap[walletId] != null)
            {
                result = _tokenStorage.WalletNftTokensMap[walletId];

                result.Sort();
            }

            return await Task.FromResult(result);
        }

        /// <summary>
        /// Find the wallet which is owning the token.
        /// </summary>
        public async Task<string> FindWalletOwnerAsync(string tokenId)
        {
            if (_tokenStorage.NftTokenWalletMap.ContainsKey(tokenId))
            {
                return await Task<string>.FromResult(_tokenStorage.NftTokenWalletMap[tokenId]);
            }

            return null;
        }

        /// <summary>
        /// Reads JSON with transactions and executes it.
        /// </summary>
        /// <exception cref="Newtonsoft.Json.JsonReaderException">I let it fail in case of wrong JSON</exception>
        /// <returns>Number of found transactions (but may not all be executed due to validation issues).</returns>
        public async Task<int> LoadFromJsonAsync(string json)
        {
            JToken token = null;

            token = JToken.Parse(json);

            if (token is JArray)
            {
                return await RebuildInMemoryStateAsync(JArray.Parse(json));
            }
            else if (token is JObject)
            {
                return await RebuildInMemoryStateAsync(JObject.Parse(json));
            }

            return 0;
        }

        /// <summary>
        /// Reads file with transactions and executes it.
        /// </summary>
        /// <exception cref="Newtonsoft.Json.JsonReaderException">I let it fail in case of wrong JSON file</exception>
        /// <returns>Number of found transactions (but may not all be executed due to validation issues).</returns>
        public async Task<int> LoadFromFileAsync(FileInfo file)
        {
            var data = await File.ReadAllTextAsync(file.FullName);

            return await LoadFromJsonAsync(data);
        }

        #endregion

        #region Private members

        private async Task<int> RebuildInMemoryStateAsync(JObject transactionJson)
        {
            if (transactionJson == null)
                return 0;

            JSchemaGenerator generator = new JSchemaGenerator();
            JSchema transactionSchema = generator.Generate(typeof(Transaction));

            if (transactionJson.IsValid(transactionSchema))
            {
                var transaction = transactionJson.ToObject<Transaction>();

                if (await ExecuteTransactionAsync(transaction))
                {
                    return 1;
                }
            }

            return 0;
        }

        private async Task<int> RebuildInMemoryStateAsync(JArray transactionsJson)
        {
            if (transactionsJson == null)
                return 0;

            JSchemaGenerator generator = new JSchemaGenerator();
            JSchema collectionSchema = generator.Generate(typeof(TransactionsList));

            if (transactionsJson.IsValid(collectionSchema))
            {
                var transactions = transactionsJson.ToObject<TransactionsList>();

                foreach (var transaction in transactions)
                {
                    await ExecuteTransactionAsync(transaction);
                }

                return transactions.Count();
            }

            return 0;
        }

        private async Task<bool> ExecuteTransactionAsync(Transaction transaction)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException("transaction");
            }

            bool result = true;
            switch (transaction.Type)
            {
                case Mint:
                    result = AddNFTToken(transaction.TokenId, transaction.Address);
                    break;
                case Burn:
                    BurnNFTToken(transaction.TokenId);
                    break;
                case Transfer:
                    result = ChangeOwnership(transaction.TokenId, transaction.From, transaction.To); // Apply change
                    break;
                default: 
                    result = false;
                    break;
            }

            return await Task.FromResult(result);
        }

        // To add an NFT token with owner to the data structure
        private bool AddNFTToken(string tokenId, string walletAddress)
        {
            if (!_tokenStorage.NftTokenWalletMap.ContainsKey(tokenId)) // I assume that tokens must be unique.
            {
                if (!_tokenStorage.WalletNftTokensMap.ContainsKey(walletAddress))
                {
                    _tokenStorage.WalletNftTokensMap.Add(walletAddress, new List<string>());
                }
                _tokenStorage.WalletNftTokensMap[walletAddress].Add(tokenId);

                _tokenStorage.NftTokenWalletMap.Add(tokenId, walletAddress);

                // Create an Ownership entry
                var nftToken = new NFTToken
                {
                    TokenId = tokenId,
                    OwnershipInfo = new LinkedList<OwnershipInfo>()
                };

                nftToken.OwnershipInfo.AddFirst(
                    new OwnershipInfo
                    {
                        WalletAddress = walletAddress,
                        Timestamp = DateTime.Now
                    });
                _tokenStorage.NftTokenOwnershipMap.Add(tokenId, nftToken);

                return true;
            }

            return false;
        }

        private void BurnNFTToken(string tokenId)
        {
            if (_tokenStorage.NftTokenWalletMap.ContainsKey(tokenId))
            {
                string walletId = _tokenStorage.NftTokenWalletMap[tokenId];

                _tokenStorage.NftTokenWalletMap.Remove(tokenId);

                if (_tokenStorage.WalletNftTokensMap.ContainsKey(walletId))
                {
                    _tokenStorage.WalletNftTokensMap.Remove(walletId);
                }
            }

            if (_tokenStorage.NftTokenOwnershipMap.ContainsKey(tokenId))
            {
                _tokenStorage.NftTokenOwnershipMap.Remove(tokenId);
            }
        }

        private bool ChangeOwnership(string tokenId, string oldWalletAddress, string newWalletAddress)
        {
            if (_tokenStorage.NftTokenWalletMap.ContainsKey(tokenId) &&
                _tokenStorage.NftTokenWalletMap[tokenId].Equals(oldWalletAddress)) // Validate that token is actually owned by From
            {
                //string oldWalletAddress = _tokenStorage.NftTokenWalletMap[tokenId];

                _tokenStorage.WalletNftTokensMap[oldWalletAddress].Remove(tokenId);
                if (!_tokenStorage.WalletNftTokensMap.ContainsKey(newWalletAddress))
                {
                    _tokenStorage.WalletNftTokensMap.Add(newWalletAddress, new List<string>());
                }
                _tokenStorage.WalletNftTokensMap[newWalletAddress].Add(tokenId);

                _tokenStorage.NftTokenWalletMap[tokenId] = newWalletAddress;

                NFTToken nftToken = _tokenStorage.NftTokenOwnershipMap[tokenId];

                nftToken.OwnershipInfo.AddFirst(
                    new OwnershipInfo
                    {
                        WalletAddress = newWalletAddress,
                        Timestamp = DateTime.Now
                    });

                return true;
            }

            return false;
        }

        #endregion
    }
}
