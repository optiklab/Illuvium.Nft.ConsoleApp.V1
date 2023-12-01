using FluentAssertions;
using Illuvium.Nft.App;
using Illuvium.Nft.App.Cli;
using Xunit;

namespace Illuvium.Nft.Tests
{
    public class ConsoleOutputHandlersTests
    {
        private const string MintDTransaction = "{ \"Type\": \"Mint\", \"TokenId\": \"0xD000000000000000000000000000000000000000\", \"Address\": \"0x1000000000000000000000000000000000000000\" }";

        [Fact]
        public async Task Successfully_Loads_From_Valid_Json()
        {
            string result = await ConsoleOutputHandlers.ReadJsonAsync(MintDTransaction, new TransactionsManager(new FileSystemMock()));

            result.Should().BeEquivalentTo("Read 1 transaction(s)");
        }

        private const string MintThenBurnTransactions = "[{\"Type\": \"Mint\",\"TokenId\": \"0xA000000000000000000000000000000000000000\",\"Address\": \"0x1000000000000000000000000000000000000000\"},{\"Type\": \"Burn\", \"TokenId\": \"0xA000000000000000000000000000000000000000\"}]";
        [Fact]
        public async Task Successfully_Loads_Mint_And_Burns_From_Valid_JsonArray()
        {
            string result = await ConsoleOutputHandlers.ReadJsonAsync(MintThenBurnTransactions, new TransactionsManager(new FileSystemMock()));

            result.Should().BeEquivalentTo("Read 2 transaction(s)");
        }

        private const string MintBurnAndTransferTransactions = "[\r\n\t{\r\n\t\t\"Type\": \"Mint\",\r\n\t\t\"TokenId\": \"0xA000000000000000000000000000000000000000\",\r\n\t\t\"Address\": \"0x1000000000000000000000000000000000000000\"\r\n\t},\r\n\t{\r\n\t\t\"Type\": \"Mint\",\r\n\t\t\"TokenId\": \"0xB000000000000000000000000000000000000000\",\r\n\t\t\"Address\": \"0x2000000000000000000000000000000000000000\"\r\n\t},\r\n\t{\r\n\t\t\"Type\": \"Mint\",\r\n\t\t\"TokenId\": \"0xC000000000000000000000000000000000000000\",\r\n\t\t\"Address\": \"0x3000000000000000000000000000000000000000\"\r\n\t},\r\n\t{\r\n\t\t\"Type\": \"Burn\",\r\n\t\t\"TokenId\": \"0xA000000000000000000000000000000000000000\"\r\n\t},\r\n\t{\r\n\t\t\"Type\": \"Transfer\",\r\n\t\t\"TokenId\": \"0xB000000000000000000000000000000000000000\",\r\n\t\t\"From\": \"0x2000000000000000000000000000000000000000\",\r\n\t\t\"To\": \"0x3000000000000000000000000000000000000000\"\r\n\t}\r\n]";
        [Fact]
        public async Task Successfully_Loads_Mint_Burn_And_Transfer_From_Valid_JsonArray()
        {
            string result = await ConsoleOutputHandlers.ReadJsonAsync(MintBurnAndTransferTransactions, new TransactionsManager(new FileSystemMock()));

            result.Should().BeEquivalentTo("Read 5 transaction(s)");
        }

        [Fact]
        public async Task Successfully_Loads_From_File()
        {
            string result = await ConsoleOutputHandlers.ReadFileAsync(new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\transactions.json"), new TransactionsManager(new FileSystemMock()));

            result.Should().BeEquivalentTo("Read 5 transaction(s)");
        }

        [Fact]
        public void Throws_Exception_When_No_File()
        {
            var act = async () => await ConsoleOutputHandlers.ReadFileAsync(new FileInfo("no.json"), new TransactionsManager(new FileSystemMock()));

            var exception = Assert.ThrowsAsync<ArgumentException>(act);

            Assert.Equal("file does not exists", exception.Result.Message);
        }

        [Fact]
        public async Task Successfully_Loads_Mint_Burn_And_Transfer_From_Valid_JsonArray1()
        {
            var fileSystemMock = new FileSystemMock();

            string result = await ConsoleOutputHandlers.ReadJsonAsync(MintBurnAndTransferTransactions, new TransactionsManager(fileSystemMock));

            result.Should().BeEquivalentTo("Read 5 transaction(s)");

            result = await ConsoleOutputHandlers.ShowOwnerAsync("0xA000000000000000000000000000000000000000", new TransactionsManager(fileSystemMock));
            result.Should().BeEquivalentTo("Token 0xA000000000000000000000000000000000000000 is not owned by any wallet");

            result = await ConsoleOutputHandlers.ShowOwnerAsync("0xB000000000000000000000000000000000000000", new TransactionsManager(fileSystemMock));
            result.Should().BeEquivalentTo("Token 0xB000000000000000000000000000000000000000 is owned by 0x3000000000000000000000000000000000000000");

            result = await ConsoleOutputHandlers.ShowOwnerAsync("0xC000000000000000000000000000000000000000", new TransactionsManager(fileSystemMock));
            result.Should().BeEquivalentTo("Token 0xC000000000000000000000000000000000000000 is owned by 0x3000000000000000000000000000000000000000");

            result = await ConsoleOutputHandlers.ShowOwnerAsync("0xD000000000000000000000000000000000000000", new TransactionsManager(fileSystemMock));
            result.Should().BeEquivalentTo("Token 0xD000000000000000000000000000000000000000 is not owned by any wallet");

            result = await ConsoleOutputHandlers.ReadJsonAsync(MintDTransaction, new TransactionsManager(fileSystemMock));
            result.Should().BeEquivalentTo("Read 1 transaction(s)");

            result = await ConsoleOutputHandlers.ShowOwnerAsync("0xD000000000000000000000000000000000000000", new TransactionsManager(fileSystemMock));
            result.Should().BeEquivalentTo("Token 0xD000000000000000000000000000000000000000 is owned by 0x1000000000000000000000000000000000000000");

            result = await ConsoleOutputHandlers.ReportTokensAsync("0x3000000000000000000000000000000000000000", new TransactionsManager(fileSystemMock));
            result.Should().StartWith("Wallet 0x3000000000000000000000000000000000000000 holds 2 Tokens:");
            result.Should().Contain("0xB000000000000000000000000000000000000000");
            result.Should().Contain("0xC000000000000000000000000000000000000000");

            result = await ConsoleOutputHandlers.ResetAsync(new TransactionsManager(fileSystemMock));
            result.Should().BeEquivalentTo("Program was reset");

            result = await ConsoleOutputHandlers.ReportTokensAsync("0x3000000000000000000000000000000000000000", new TransactionsManager(fileSystemMock));
            result.Should().StartWith("Wallet 0x3000000000000000000000000000000000000000 holds no Tokens");
        }
    }
}