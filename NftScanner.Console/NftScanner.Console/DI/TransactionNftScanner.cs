using Blockfrost.Api;
using Microsoft.Extensions.Logging;
using NftScanner.Console.DI.Interfases;
using NftScanner.Console.Models;

namespace NftScanner.Console.DI;

public class TransactionNftScanner : ITransactionNftScanner
{
    private readonly IStorage _storage;
    private readonly IAssetService _assetService;
    private readonly ITransactionService _transactionService;
    private readonly ILogger<TransactionNftScanner> _logger;
    private readonly List<string> _transactionsAmountIgnore = new() { "lovelace" };

    public TransactionNftScanner(ITransactionService transactionService, IAssetService assetService, ILogger<TransactionNftScanner> logger, IStorage storage)
    {
        _transactionService = transactionService;
        _assetService = assetService;
        _logger = logger;
        _storage = storage;
    }

    public async Task<IEnumerable<FileStorageResult>> ProcessTransaction(string transactionHash)
    {
        _logger.LogInformation("Start to process  transaction, hash: {transactionHash}", transactionHash);
        var txContent = await _transactionService.UtxosAsync(transactionHash);
        var inputUnits = txContent.Inputs.SelectMany(a => a.Amount).Where(a =>
                !_transactionsAmountIgnore.Exists(
                    item => item.Equals(a.Unit, StringComparison.CurrentCultureIgnoreCase)) &&
                a.Quantity.Equals("1", StringComparison.CurrentCultureIgnoreCase))
            .Select(a => a.Unit);
        var outputUnits = txContent.Outputs.SelectMany(a => a.Amount).Where(a =>
                !_transactionsAmountIgnore.Exists(
                    item => item.Equals(a.Unit, StringComparison.CurrentCultureIgnoreCase)) &&
                a.Quantity.Equals("1", StringComparison.CurrentCultureIgnoreCase))
            .Select(a => a.Unit);
        var assetUnits = inputUnits.Union(outputUnits);
        var assetsMetadata = await GetAssetMetadata(assetUnits);
        return await _storage.SaveAssets(transactionHash, assetsMetadata);
    }

    private async Task<IEnumerable<Onchain_metadata>> GetAssetMetadata(IEnumerable<string> assetUnits)
    {
        ParallelOptions parallelOptions = new()
        {
            MaxDegreeOfParallelism = 5
        };
        List<Onchain_metadata> result = new();
        await Parallel.ForEachAsync(assetUnits, parallelOptions, async (unit, token) =>
        {
            try
            {
                _logger.LogInformation("Start to get asset for unit: {unit}", unit);
                var assets = await _assetService.AssetsAsync(unit, token);
                if (assets is {Onchain_metadata: not null})
                {
                    result.Add(assets.Onchain_metadata);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while get asset info");
            }
        });
        return result;
    }
}