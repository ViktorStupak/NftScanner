using NftScanner.Console.Models;

namespace NftScanner.Console.DI.Interfases;

public interface ITransactionNftScanner
{
    public Task<IEnumerable<FileStorageResult>> ProcessTransaction(string transactionHash);
}