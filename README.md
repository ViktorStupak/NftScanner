# Transaction NFT Scanner

Decentralized apps offer transparency, allowing users to monitor internal activities. Leveraging this, we aim to create an application to display NFT images involved in a transaction.

## Blockfrost API Usage

To gather transaction and asset information, utilize the Blockfrost API. If needed, use the provided apiKey in the header.

### Blockfrost API Endpoints:

- [Transaction UTXOs](https://blockfrost.dev/api/transaction-utx-os)
- [Specific Asset](https://blockfrost.dev/api/specific-asset)

## IPFS Gateway

Retrieve images using an IPFS gateway provider like Cloudflare IPFS. Images can be accessed via URL in the format: `https://cloudflare-ipfs.com/ipfs/{ipfsHash}`.

## NFT Detection

Detect NFTs related to transaction inputs and outputs. For simplicity, consider an asset an NFT if its quantity is 1 and its unit is not "lovelace".

## Project Structure

- Include a folder named "Transactions".
- For each scanned transaction, create a folder named after the transaction hash.
- Inside this folder, download images of all NFTs included in the transaction. Name each image after the NFT it displays.

## How to Use the App

> Utilize `Microsoft.Extensions.Configuration.UserSecrets` for configuration. See more in the [Microsoft Documentation](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-8.0&tabs=windows).

The entry point is the implementation of `ITransactionNftScanner` in the DI container. Example usage:

```csharp
var scanner = services.GetRequiredService<ITransactionNftScanner>();
await scanner.ProcessTransaction(transactionHash);
```
Configuration's example
``` json
{
  "BlockfrostOptions": {
    "Network": "net",
    "ApiKey": "yourApiKey"
  },
  "StorageOptions": {
    "RootFolder": "Transactions"
  },
  "IpfsGatewayProviderOptions": {
    "BaseUrl": "https://cloudflare-ipfs.com"
  }
}
```
