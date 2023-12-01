namespace Illuvium.Nft.App
{
    public interface ITransactionsManager
    {
        Task<bool> InitializeAsync();

        Task<bool> ClearAsync();

        Task<bool> PersistAsync();

        Task<List<string>> GetTokensAsync(string walletId);

        Task<string> FindWalletOwnerAsync(string tokenId);

        Task<int> LoadFromJsonAsync(string json);

        Task<int> LoadFromFileAsync(FileInfo file);
    }
}
