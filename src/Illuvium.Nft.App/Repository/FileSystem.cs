using Illuvium.Nft.App.Models;
using System.Runtime.Serialization.Formatters.Binary;

namespace Illuvium.Nft.App.Repository
{
    /// <summary>
    /// Implements most primitive way to persist the state.
    /// Though, this way is NOT something I would use for production: 
    /// https://learn.microsoft.com/en-us/dotnet/standard/serialization/binaryformatter-security-guide
    /// </summary>
    public class FileSystem : IFileSystem
    {
        private const string IlluviumDb = "IlluviumDb";

        public async Task<TokenStorage> ReadAsync()
        {
            if (File.Exists(IlluviumDb))
            {
                var formatter = new BinaryFormatter();

                using (var stream = new FileStream(IlluviumDb, FileMode.Open, FileAccess.Read))
                {
                    return await Task<TokenStorage>.FromResult((TokenStorage)formatter.Deserialize(stream));
                }
            }

            return null;
        }

        public async Task<bool> DeleteAsync()
        {
            if (File.Exists(IlluviumDb))
            {
                File.Delete(IlluviumDb);

                return await Task<bool>.FromResult(true);
            }

            return await Task<bool>.FromResult(false);
        }

        public async Task<bool> SaveAsync(TokenStorage tokenStorage)
        {
            bool result = true;

            var formatter = new BinaryFormatter();

            try
            {
                using (var stream = new FileStream(IlluviumDb, FileMode.Create, FileAccess.Write))
                {
                    formatter.Serialize(stream, tokenStorage);
                    stream.Close();
                }
            }
            catch
            {
                result = false;
            }

            return await Task<bool>.FromResult(result);
        }
    }
}
