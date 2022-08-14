using Nest;
using SiteSearch.Core;
using SiteSearch.Core.Extensions;
using SiteSearch.Core.Interfaces;
using SiteSearch.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SiteSearch.Elastic
{
    public class ElasticSearchIndex<T> : BaseSearchIndex<T>, ISearchIndex<T> where T : class, new()
    {
        private readonly string indexType;
        private readonly IElasticClient elasticClient;

        public ElasticSearchIndex() : base()
        {
            indexType = typeof(T).AssemblyQualifiedName.SafeFilename();
        }

        public ElasticSearchIndex(IElasticClient elasticClient) : this()
        {
            this.elasticClient = elasticClient ?? throw new ArgumentNullException(nameof(elasticClient));
        }

        public Task CreateIndexAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(string id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task IndexAsync(T document, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task IndexAsync(IEnumerable<T> documents, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<SearchResult<T>> SearchAsync(SearchQuery query, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
