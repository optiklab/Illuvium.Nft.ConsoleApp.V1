using System.CommandLine;
using Illuvium.Nft.App.Cli;

class Program
{
    static async Task<int> Main(string[] args)
    {
        return await CliRoot.Initialize().InvokeAsync(args);
    }
}