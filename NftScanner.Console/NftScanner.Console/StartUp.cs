using System.Reflection;
using Blockfrost.Api.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NftScanner.Console.DI;
using NftScanner.Console.DI.Interfases;
using NftScanner.Console.Options;

namespace NftScanner.Console;

public class StartUp
{
    public static ServiceProvider CreateServices()
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets(Assembly.GetExecutingAssembly())
            .Build();

        var serviceCollection = new ServiceCollection()
            .AddOptions()
            .Configure<StorageOptions>(configuration.GetSection(nameof(StorageOptions)))
            .AddSingleton<IStorage, Storage>()
            .AddSingleton<ITransactionNftScanner, TransactionNftScanner>()
            .AddLogging(opt => opt.AddConsole());
        var configOptions = configuration.GetSection(nameof(BlockfrostOptions));
        var configInstance = configOptions.Get<BlockfrostOptions>();
        serviceCollection.AddBlockfrost(configInstance?.Network, configInstance?.ApiKey);
        serviceCollection.AddHttpClient<IStorage, Storage>(client =>
        {
            var gatewayProviderOptions = configuration.GetSection(nameof(IpfsGatewayProviderOptions));
            var gatewayProviderOptionsInstance = gatewayProviderOptions.Get<IpfsGatewayProviderOptions>();
            client.BaseAddress = new Uri(gatewayProviderOptionsInstance?.BaseUrl ?? throw new InvalidOperationException("Please specify IPFS gateway URL"));
        });
        return serviceCollection.BuildServiceProvider();
    }
}