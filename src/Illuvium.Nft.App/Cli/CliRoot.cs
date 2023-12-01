using System.CommandLine;

namespace Illuvium.Nft.App.Cli
{
    /// <summary>
    /// Responsible for Console Input and Output.
    /// </summary>
    public static class CliRoot
    {
        /// <summary>
        /// Initializes commands patterns using dotnet functionality
        /// https://learn.microsoft.com/en-us/dotnet/standard/commandline/get-started-tutorial
        /// https://learn.microsoft.com/en-us/dotnet/standard/commandline/
        /// 
        /// NOTE: In case it would have much more commands, I would split it to classes and implemented some Reflection-based mechanism to automatically register commands.
        /// </summary>
        public static RootCommand Initialize()
        {
            var rootCommand = new RootCommand("Illuvium app to work with NFT tokens.");

            var fileCommand = new Command("--read-file", "Reads transactions from the ﬁle in the speciﬁed location.");
            var fileAgrument = new Argument<FileInfo>("file", "File path to read.");
            fileCommand.AddArgument(fileAgrument);
            fileCommand.SetHandler(async (file, transactionsManager) => await ReadFileAsync(file!, transactionsManager), fileAgrument, new TransactionsManagerBinder());

            var readInlineCommand = new Command("--read-inline", "Reads either a single json element, or an array of json elements representing transactions as an argument.");
            var readInlineAgrument = new Argument<string>("json", "Json text.");
            readInlineCommand.AddArgument(readInlineAgrument);
            readInlineCommand.SetHandler(async (json, transactionsManager) => await ReadJsonAsync(json!, transactionsManager), readInlineAgrument, new TransactionsManagerBinder());

            var nftCommand = new Command("--nft", "Returns ownership information for the nft with the given id.");
            var nftAgrument = new Argument<string>("tokenId", "NFT token Id.");
            nftCommand.AddArgument(nftAgrument);
            nftCommand.SetHandler(async (tokenId, transactionsManager) => await ShowOwnerAsync(tokenId!, transactionsManager), nftAgrument, new TransactionsManagerBinder());

            var walletCommand = new Command("--wallet", "Lists all NFTs currently owned by the wallet of the given address.");
            var walletAgrument = new Argument<string>("Address", "Wallet address.");
            walletCommand.AddArgument(walletAgrument);
            walletCommand.SetHandler(async (address, transactionsManager) => await ReportTokensAsync(address!, transactionsManager), walletAgrument, new TransactionsManagerBinder());

            var resetCommand = new Command("--reset", "Deletes all data previously processed by the program.");
            resetCommand.SetHandler(async (transactionsManager) => await ResetAsync(transactionsManager), new TransactionsManagerBinder());

            rootCommand.AddCommand(fileCommand);
            rootCommand.AddCommand(readInlineCommand);
            rootCommand.AddCommand(nftCommand);
            rootCommand.AddCommand(walletCommand);
            rootCommand.AddCommand(resetCommand);

            return rootCommand;
        }

        static async Task ReadFileAsync(FileInfo file, ITransactionsManager transactionsManager)
        {
            Console.WriteLine(await ConsoleOutputHandlers.ReadFileAsync(file, transactionsManager));
        }

        static async Task ReadJsonAsync(string json, ITransactionsManager transactionsManager)
        {
            Console.WriteLine(await ConsoleOutputHandlers.ReadJsonAsync(json, transactionsManager));
        }

        static async Task ShowOwnerAsync(string tokenId, ITransactionsManager transactionsManager)
        {
            Console.WriteLine(await ConsoleOutputHandlers.ShowOwnerAsync(tokenId, transactionsManager));
        }

        static async Task ReportTokensAsync(string address, ITransactionsManager transactionsManager)
        {
            Console.WriteLine(await ConsoleOutputHandlers.ReportTokensAsync(address, transactionsManager));
        }

        static async Task ResetAsync(ITransactionsManager transactionsManager)
        {
            Console.WriteLine(await ConsoleOutputHandlers.ResetAsync(transactionsManager));
        }
    }
}
