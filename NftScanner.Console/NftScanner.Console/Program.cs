// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.DependencyInjection;
using NftScanner.Console;
using NftScanner.Console.DI.Interfases;

Console.WriteLine("Hello, Transaction Nft Scanner!");

var services = StartUp.CreateServices();
var scanner = services.GetRequiredService<ITransactionNftScanner>();

while (true)
{
    Console.WriteLine("Please print your transaction hash:");
    var transactionHash = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(transactionHash))continue;
    var scannerResults = await scanner.ProcessTransaction(transactionHash);
    foreach (var result in scannerResults)
    {
        Console.WriteLine($"Save asset {result.FileName} into Path: {result.Path}");
    }

}