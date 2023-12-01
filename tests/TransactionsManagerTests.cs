using FluentAssertions;
using Illuvium.Nft.App;
using Illuvium.Nft.App.Repository;
using Xunit;

namespace Illuvium.Nft.Tests;

public class TransactionsManagerTests
{
    private const string MintDTransaction = "{ \"Type\": \"Mint\", \"TokenId\": \"0xD000000000000000000000000000000000000000\", \"Address\": \"0x1000000000000000000000000000000000000000\" }";

    IFileSystem _fileSystemMock = new FileSystemMock();

    [Fact]
    public async Task Successfully_Loads_Mint_From_Valid_Json()
    {
        var manager = new TransactionsManager(_fileSystemMock);

        int result = await manager.LoadFromJsonAsync(MintDTransaction);

        result.Should().Be(1);

        string walletId = await manager.FindWalletOwnerAsync("0xD000000000000000000000000000000000000000");

        walletId.Should().BeEquivalentTo("0x1000000000000000000000000000000000000000");

        var tokens = await manager.GetTokensAsync(walletId);

        tokens.Should().NotBeNull();
        tokens.First().Should().BeEquivalentTo("0xD000000000000000000000000000000000000000");
    }

    private const string MintThenBurnTransactions = "[{\"Type\": \"Mint\",\"TokenId\": \"0xA000000000000000000000000000000000000000\",\"Address\": \"0x1000000000000000000000000000000000000000\"},{\"Type\": \"Burn\", \"TokenId\": \"0xA000000000000000000000000000000000000000\"}]";
    [Fact]
    public async Task Successfully_Loads_Mint_And_Burns_From_Valid_JsonArray()
    {
        var manager = new TransactionsManager(_fileSystemMock);

        int result = await manager.LoadFromJsonAsync(MintThenBurnTransactions);

        result.Should().Be(2);

        string walletId = await manager.FindWalletOwnerAsync("0xA000000000000000000000000000000000000000");

        walletId.Should().BeNull();
    }

    private const string MintBurnAndTransferTransactions = "[\r\n\t{\r\n\t\t\"Type\": \"Mint\",\r\n\t\t\"TokenId\": \"0xA000000000000000000000000000000000000000\",\r\n\t\t\"Address\": \"0x1000000000000000000000000000000000000000\"\r\n\t},\r\n\t{\r\n\t\t\"Type\": \"Mint\",\r\n\t\t\"TokenId\": \"0xB000000000000000000000000000000000000000\",\r\n\t\t\"Address\": \"0x2000000000000000000000000000000000000000\"\r\n\t},\r\n\t{\r\n\t\t\"Type\": \"Mint\",\r\n\t\t\"TokenId\": \"0xC000000000000000000000000000000000000000\",\r\n\t\t\"Address\": \"0x3000000000000000000000000000000000000000\"\r\n\t},\r\n\t{\r\n\t\t\"Type\": \"Burn\",\r\n\t\t\"TokenId\": \"0xA000000000000000000000000000000000000000\"\r\n\t},\r\n\t{\r\n\t\t\"Type\": \"Transfer\",\r\n\t\t\"TokenId\": \"0xB000000000000000000000000000000000000000\",\r\n\t\t\"From\": \"0x2000000000000000000000000000000000000000\",\r\n\t\t\"To\": \"0x3000000000000000000000000000000000000000\"\r\n\t}\r\n]";

    [Fact]
    public async Task Successfully_Loads_Mint_Burn_And_Transfer_From_Valid_JsonArray()
    {
        var manager = new TransactionsManager(_fileSystemMock);

        int result = await manager.LoadFromJsonAsync(MintBurnAndTransferTransactions);

        result.Should().Be(5);

        string walletId = await manager.FindWalletOwnerAsync("0xA000000000000000000000000000000000000000");
        walletId.Should().BeNull();

        walletId = await manager.FindWalletOwnerAsync("0xB000000000000000000000000000000000000000");
        walletId.Should().BeEquivalentTo("0x3000000000000000000000000000000000000000");

        walletId = await manager.FindWalletOwnerAsync("0xC000000000000000000000000000000000000000");
        walletId.Should().BeEquivalentTo("0x3000000000000000000000000000000000000000");

        walletId = await manager.FindWalletOwnerAsync("0xD000000000000000000000000000000000000000");
        walletId.Should().BeNull();

        result = await manager.LoadFromJsonAsync(MintDTransaction);
        result.Should().Be(1);

        walletId = await manager.FindWalletOwnerAsync("0xD000000000000000000000000000000000000000");

        walletId.Should().BeEquivalentTo("0x1000000000000000000000000000000000000000");

        var tokens = await manager.GetTokensAsync(walletId);

        tokens.Should().NotBeNull();
        tokens.First().Should().BeEquivalentTo("0xD000000000000000000000000000000000000000");

        tokens = await manager.GetTokensAsync("0x3000000000000000000000000000000000000000");
        tokens.Should().NotBeNull();
        tokens[0].Should().BeEquivalentTo("0xB000000000000000000000000000000000000000");
        tokens[1].Should().BeEquivalentTo("0xC000000000000000000000000000000000000000");

        bool isOk = await manager.ClearAsync(); // Reset
        isOk.Should().BeTrue();

        tokens = await manager.GetTokensAsync("0x3000000000000000000000000000000000000000");
        tokens.Should().NotBeNull();
        tokens.Should().BeEmpty();
    }

    [Fact]
    public async Task Successfully_Loads_From_No_File()
    {
        var manager = new TransactionsManager(_fileSystemMock);

        int result = await manager.LoadFromFileAsync(new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "\\transactions.json"));

        result.Should().Be(5);
    }

    [Fact]
    public void Gracefully_Fails_On_Invalid_Json()
    {
        var manager = new TransactionsManager(_fileSystemMock);

        var act = async () => await manager.LoadFromJsonAsync("[}");

        var exception = Assert.ThrowsAsync<Newtonsoft.Json.JsonReaderException>(act);

        Assert.StartsWith("Unexpected character", exception.Result.Message);
    }
}
