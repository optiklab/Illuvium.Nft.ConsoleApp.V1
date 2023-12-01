using Illuvium.Nft.App.Models;

namespace Illuvium.Nft.App.Repository
{
    public interface IFileSystem
    {
        Task<TokenStorage> ReadAsync();

        Task<bool> DeleteAsync();

        Task<bool> SaveAsync(TokenStorage tokenStorage);
    }
}
