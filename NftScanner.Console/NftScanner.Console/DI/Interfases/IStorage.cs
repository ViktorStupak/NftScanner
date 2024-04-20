using Blockfrost.Api;
using NftScanner.Console.Models;

namespace NftScanner.Console.DI.Interfases;

public interface IStorage
{
    public Task<IEnumerable<FileStorageResult>> SaveAssets(string transactionHash, IEnumerable<Onchain_metadata> assetMetadata);
}