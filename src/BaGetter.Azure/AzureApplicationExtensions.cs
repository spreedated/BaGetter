using System;
using Azure.Data.Tables;
using Azure.Storage;
using Azure.Storage.Blobs;
using BaGetter.Azure;
using BaGetter.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace BaGetter
{
    public static class AzureApplicationExtensions
    {
        public static BaGetterApplication AddAzureTableDatabase(this BaGetterApplication app)
        {
            app.Services.AddBaGetterOptions<AzureTableOptions>(nameof(BaGetterOptions.Database));

            app.Services.AddTransient<TablePackageDatabase>();
            app.Services.AddTransient<TableSearchService>();
            app.Services.TryAddTransient<IPackageDatabase>(provider => provider.GetRequiredService<TablePackageDatabase>());
            app.Services.TryAddTransient<ISearchService>(provider => provider.GetRequiredService<TableSearchService>());
            app.Services.TryAddTransient<ISearchIndexer>(provider => provider.GetRequiredService<NullSearchIndexer>());

            app.Services.AddSingleton(provider =>
            {
                var options = provider.GetRequiredService<IOptions<AzureTableOptions>>().Value;

                var tableServiceClient = new TableServiceClient(options.ConnectionString);
                tableServiceClient.CreateTableIfNotExists(options.TableName);
                return tableServiceClient;
            });

            app.Services.AddProvider<IPackageDatabase>((provider, config) =>
            {
                if (!config.HasDatabaseType("AzureTable")) return null;

                return provider.GetRequiredService<TablePackageDatabase>();
            });

            app.Services.AddProvider<ISearchService>((provider, config) =>
            {
                if (!config.HasSearchType("Database")) return null;
                if (!config.HasDatabaseType("AzureTable")) return null;

                return provider.GetRequiredService<TableSearchService>();
            });

            app.Services.AddProvider<ISearchIndexer>((provider, config) =>
            {
                if (!config.HasSearchType("Database")) return null;
                if (!config.HasDatabaseType("AzureTable")) return null;

                return provider.GetRequiredService<NullSearchIndexer>();
            });

            return app;
        }

        public static BaGetterApplication AddAzureTableDatabase(
            this BaGetterApplication app,
            Action<AzureTableOptions> configure)
        {
            app.AddAzureTableDatabase();
            app.Services.Configure(configure);
            return app;
        }

        public static BaGetterApplication AddAzureBlobStorage(this BaGetterApplication app)
        {
            app.Services.AddBaGetterOptions<AzureBlobStorageOptions>(nameof(BaGetterOptions.Storage));
            app.Services.AddTransient<BlobStorageService>();
            app.Services.TryAddTransient<IStorageService>(provider => provider.GetRequiredService<BlobStorageService>());

            app.Services.AddSingleton(provider =>
            {
                var options = provider.GetRequiredService<IOptions<AzureBlobStorageOptions>>().Value;

                // TODO: Add BlobClientOptions with customer-provided key.
                if (!string.IsNullOrEmpty(options.ConnectionString))
                {
                    return new BlobServiceClient(options.ConnectionString);
                }

                return new BlobServiceClient(new Uri($"https://{options.AccountName}.blob.core.windows.net"), new StorageSharedKeyCredential(options.AccountName, options.AccessKey));
            });

            app.Services.AddTransient(provider =>
            {
                var options = provider.GetRequiredService<IOptionsSnapshot<AzureBlobStorageOptions>>().Value;
                var account = provider.GetRequiredService<BlobServiceClient>();

                return account.GetBlobContainerClient(options.Container);
            });

            app.Services.AddProvider<IStorageService>((provider, config) =>
            {
                if (!config.HasStorageType("AzureBlobStorage")) return null;

                return provider.GetRequiredService<BlobStorageService>();
            });

            return app;
        }

        public static BaGetterApplication AddAzureBlobStorage(
            this BaGetterApplication app,
            Action<AzureBlobStorageOptions> configure)
        {
            app.AddAzureBlobStorage();
            app.Services.Configure(configure);
            return app;
        }

        public static BaGetterApplication AddAzureSearch(this BaGetterApplication app)
        {
            throw new NotImplementedException();

            //app.Services.AddBaGetterOptions<AzureSearchOptions>(nameof(BaGetterOptions.Search));

            //app.Services.AddTransient<AzureSearchBatchIndexer>();
            //app.Services.AddTransient<AzureSearchService>();
            //app.Services.AddTransient<AzureSearchIndexer>();
            //app.Services.AddTransient<IndexActionBuilder>();
            //app.Services.TryAddTransient<ISearchService>(provider => provider.GetRequiredService<AzureSearchService>());
            //app.Services.TryAddTransient<ISearchIndexer>(provider => provider.GetRequiredService<AzureSearchIndexer>());

            //app.Services.AddSingleton(provider =>
            //{
            //    var options = provider.GetRequiredService<IOptions<AzureSearchOptions>>().Value;
            //    var credentials = new SearchCredentials(options.ApiKey);

            //    return new SearchServiceClient(options.AccountName, credentials);
            //});

            //app.Services.AddSingleton(provider =>
            //{
            //    var options = provider.GetRequiredService<IOptions<AzureSearchOptions>>().Value;
            //    var credentials = new SearchCredentials(options.ApiKey);

            //    return new SearchIndexClient(options.AccountName, PackageDocument.IndexName, credentials);
            //});

            //app.Services.AddProvider<ISearchService>((provider, config) =>
            //{
            //    if (!config.HasSearchType("AzureSearch")) return null;

            //    return provider.GetRequiredService<AzureSearchService>();
            //});

            //app.Services.AddProvider<ISearchIndexer>((provider, config) =>
            //{
            //    if (!config.HasSearchType("AzureSearch")) return null;

            //    return provider.GetRequiredService<AzureSearchIndexer>();
            //});

            //return app;
        }

        public static BaGetterApplication AddAzureSearch(
            this BaGetterApplication app,
            Action<AzureSearchOptions> configure)
        {
            app.AddAzureSearch();
            app.Services.Configure(configure);
            return app;
        }
    }
}
