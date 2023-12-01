using System.Text;

namespace Illuvium.Nft.App.Cli
{
    /// <summary>
    /// This is fully static class that incapsulates the high-level logic needed for the expected console output.
    /// Every command usually needs to do 3 things:
    /// - Read the data persisted during previous run (if exists),
    /// - Execute the new command
    /// - Persist the new data (on disk in our case)
    /// </summary>
    public static class ConsoleOutputHandlers
    {
        private const string NoStateWarning = $"WARNING! It was not possible to recover previous state of the application. So it will run as of first time.";

        public static async Task<string> ReadFileAsync(FileInfo file, ITransactionsManager transactionsManager)
        {
            if (file == null)
                throw new ArgumentNullException("file");

            if (!file.Exists)
                throw new ArgumentException("file does not exists");

            if (!await transactionsManager.InitializeAsync())
            {
                Console.WriteLine(NoStateWarning);
            }

            int result = await transactionsManager.LoadFromFileAsync(file);

            await transactionsManager.PersistAsync();

            return $"Read {result} transaction(s)";
        }

        public static async Task<string> ReadJsonAsync(string json, ITransactionsManager transactionsManager)
        {
            if (string.IsNullOrEmpty(json))
                throw new ArgumentNullException("json");

            if (!await transactionsManager.InitializeAsync())
            {
                Console.WriteLine(NoStateWarning);
            }

            int result = await transactionsManager.LoadFromJsonAsync(json);

            await transactionsManager.PersistAsync();

            return $"Read {result} transaction(s)";
        }

        public static async Task<string> ShowOwnerAsync(string tokenId, ITransactionsManager transactionsManager)
        {
            if (string.IsNullOrEmpty(tokenId))
                throw new ArgumentNullException("tokenId");

            if (!await transactionsManager.InitializeAsync())
            {
                Console.WriteLine(NoStateWarning);
            }

            string walletId = await transactionsManager.FindWalletOwnerAsync(tokenId);

            if (string.IsNullOrEmpty(walletId))
            {
                return $"Token {tokenId} is not owned by any wallet";
            }

            return $"Token {tokenId} is owned by {walletId}";
        }

        public static async Task<string> ReportTokensAsync(string address, ITransactionsManager transactionsManager)
        {
            if (string.IsNullOrEmpty(address))
                throw new ArgumentNullException("address");

            if (!await transactionsManager.InitializeAsync())
            {
                Console.WriteLine(NoStateWarning);
            }

            var tokens = await transactionsManager.GetTokensAsync(address);

            if (tokens.Any())
            {
                var sb = new StringBuilder($"Wallet {address} holds {tokens.Count} Tokens:");

                foreach (var token in tokens)
                {
                    sb.AppendLine(token);
                }

                return sb.ToString();
            }

            return $"Wallet {address} holds no Tokens";
        }

        public static async Task<string> ResetAsync(ITransactionsManager transactionsManager)
        {
            if (!await transactionsManager.InitializeAsync())
            {
                Console.WriteLine(NoStateWarning);
            }

            if (await transactionsManager.ClearAsync())
            {
                return "Program was reset";
            }

            return "Oops. Something went wrong.";
        }
    }
}
