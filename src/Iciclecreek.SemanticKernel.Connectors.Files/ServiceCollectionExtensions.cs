using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;

namespace Iciclecreek.SemanticKernel.Connectors.Files
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Register an File <see cref="VectorStore"/> with the specified service ID.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to register the <see cref="VectorStore"/> on.</param>
        /// <param name="options">Optional options to further configure the <see cref="VectorStore"/>.</param>
        /// <param name="serviceId">An optional service id to use as the service key.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddFileVectorStore(this IServiceCollection services, FileVectorStoreOptions? options = default, string? serviceId = default)
        {
            services.AddKeyedTransient<VectorStore>(
                serviceId,
                (sp, obj) =>
                {
                    options ??= sp.GetService<FileVectorStoreOptions>() ?? new FileVectorStoreOptions()
                    {
                        EmbeddingGenerator = sp.GetService<IEmbeddingGenerator>()
                    };

                    return new FileVectorStore(options);
                });

            if (options != null)
                services.AddSingleton<FileVectorStoreOptions>(options);
            else
                services.AddSingleton<FileVectorStoreOptions>();

            services.AddKeyedSingleton<FileVectorStore, FileVectorStore>(serviceId);
            services.AddKeyedSingleton<VectorStore>(serviceId, (sp, obj) => sp.GetRequiredKeyedService<FileVectorStore>(serviceId));
            return services;
        }

        /// <summary>
        /// Register an File <see cref="VectorStoreCollection{TKey, TRecord}"/> and <see cref="IVectorSearchable{TRecord}"/> with the specified service ID.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TRecord">The type of the record.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to register the <see cref="VectorStoreCollection{TKey, TRecord}"/> on.</param>
        /// <param name="collectionName">The name of the collection.</param>
        /// <param name="serviceId">An optional service id to use as the service key.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddFileVectorStoreRecordCollection<TKey, TRecord>(
            this IServiceCollection services,
            string collectionName,
            string? serviceId = default)
            where TKey : notnull
            where TRecord : class
        {
            services.AddKeyedSingleton<VectorStoreCollection<TKey, TRecord>>(
                serviceId,
                (sp, obj) =>
                {
                    var vectoreStore = sp.GetService<FileVectorStore>();
                    return vectoreStore.GetCollection<TKey, TRecord>(collectionName)!;
                });

            services.AddKeyedSingleton<IVectorSearchable<TRecord>>(
                serviceId,
                (sp, obj) =>
                {
                    return sp.GetRequiredKeyedService<VectorStoreCollection<TKey, TRecord>>(serviceId);
                });

            return services;
        }
    }
}
