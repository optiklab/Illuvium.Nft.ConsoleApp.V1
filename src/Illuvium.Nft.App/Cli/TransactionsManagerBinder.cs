using System.CommandLine.Binding;
using Illuvium.Nft.App.Repository;

namespace Illuvium.Nft.App.Cli
{
    /// <summary>
    /// Class responsible for dependency injection of the short-lived applications, like command line apps.
    /// 
    /// https://learn.microsoft.com/en-us/dotnet/standard/commandline/dependency-injection
    /// </summary>
    public class TransactionsManagerBinder : BinderBase<ITransactionsManager>
    {
        protected override ITransactionsManager GetBoundValue(BindingContext bindingContext) => GetTransactionsManager(bindingContext);

        ITransactionsManager GetTransactionsManager(BindingContext bindingContext)
        {
            return new TransactionsManager(new FileSystem());
        }
    }
}
