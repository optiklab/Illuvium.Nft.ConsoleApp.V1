using Nft.App.Models;
using Nft.App.Repository;

namespace Nft.Tests
{
    internal class FileSystemMock : IFileSystem
    {
        private TokenStorage _tokenStorage = new TokenStorage();

        public async Task<TokenStorage> ReadAsync()
        {
            return await Task<TokenStorage>.FromResult(_tokenStorage);
        }

        public async Task<bool> DeleteAsync()
        {
            _tokenStorage = new TokenStorage();

            return await Task<bool>.FromResult(true);
        }

        public async Task<bool> SaveAsync(TokenStorage tokenStorage)
        {
            _tokenStorage = tokenStorage;

            return await Task<bool>.FromResult(true);
        }
    }
}
