using Microsoft.Extensions.AI;

namespace Iciclecreek.SemanticKernel.Connectors.Files
{
    public sealed class FileVectorStoreOptions
    {
        public string Path { get; set; } = "./vector_collections";

        //
        // Summary:
        //     Gets or sets the default embedding generator to use when generating vectors embeddings
        //     with this vector store.
        public IEmbeddingGenerator? EmbeddingGenerator { get; set; }
    }
}
