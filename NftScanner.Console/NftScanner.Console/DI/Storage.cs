using Blockfrost.Api;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NftScanner.Console.DI.Interfases;
using NftScanner.Console.Models;
using NftScanner.Console.Options;

namespace NftScanner.Console.DI;

public class Storage : IStorage
{
    private readonly ILogger<Storage> _logger;
    private readonly StorageOptions _storageOptions;
    private readonly HttpClient _httpClient;

    public Storage(ILogger<Storage> logger, IOptionsMonitor<StorageOptions> storageOptions, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
        _storageOptions = storageOptions.CurrentValue;
        if (!Directory.Exists(_storageOptions.RootFolder))
        {
            _logger.LogInformation("Create root folder: {path}", Path.GetFullPath(_storageOptions.RootFolder));
            Directory.CreateDirectory(_storageOptions.RootFolder);
        }

        storageOptions.OnChange(opt =>
        {
            _storageOptions.RootFolder = opt.RootFolder;
            if (Directory.Exists(_storageOptions.RootFolder)) return;
            _logger.LogInformation("Create root folder: {path}, after change storage option", Path.GetFullPath(_storageOptions.RootFolder));
            Directory.CreateDirectory(_storageOptions.RootFolder);
        });
    }
    public async Task<IEnumerable<FileStorageResult>> SaveAssets(string transactionHash, IEnumerable<Onchain_metadata> assetMetadata)
    {
        var folderPath = Path.Combine(_storageOptions.RootFolder, transactionHash);
        if (!Directory.Exists(folderPath))
        {
            _logger.LogInformation("Create root folder: {path}", folderPath);
            Directory.CreateDirectory(folderPath);
        }
        else
        {
            Directory.Delete(folderPath, true);
            Directory.CreateDirectory(folderPath);
        }
        List<FileStorageResult> result = new();
        ParallelOptions parallelOptions = new()
        {
            MaxDegreeOfParallelism = 3
        };

        await Parallel.ForEachAsync(assetMetadata, parallelOptions, async (metadata, token) =>
        {
            var imagePath = Path.Combine(folderPath, metadata.Name);
            var imageStream = await _httpClient.GetStreamAsync(metadata.Image.Replace("://", "/"), token);
            await using var fileStream = new FileStream(imagePath, FileMode.Create, FileAccess.Write);
            await imageStream.CopyToAsync(fileStream, token);
            result.Add(new FileStorageResult {FileName = metadata.Name, Path = imagePath});
        });
        return result;
    }
}