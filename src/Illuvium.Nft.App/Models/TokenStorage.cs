namespace Illuvium.Nft.App.Models
{
    [Serializable]
    public class TokenStorage
    {
        public TokenStorage()
        {
            NftTokenWalletMap = new Dictionary<string, string>();
            WalletNftTokensMap = new Dictionary<string, List<string>>();
            NftTokenOwnershipMap = new Dictionary<string, NFTToken>();
        }

        // To easily find owning wallet by NFT token.
        public Dictionary<string, string> NftTokenWalletMap { get; set; }
        // To easily find list of owned Tokens in the wallet.
        public Dictionary<string, List<string>> WalletNftTokensMap { get; set; }
        // To easily change the ownership.
        public Dictionary<string, NFTToken> NftTokenOwnershipMap { get; set; }
    }
}
