using SiteSearch.Core.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SiteSearch.Core.Interfaces
{
    public interface ISearchIndex<T>
    {
        Task CreateIndexAsync(CancellationToken cancellationToken = default);
        Task IndexAsync(T document, CancellationToken cancellationToken = default);
        Task IndexAsync(IEnumerable<T> documents, CancellationToken cancellationToken = default);
        Task<SearchResult<T>> SearchAsync(SearchQuery query, CancellationToken cancellationToken = default);
    }
}
