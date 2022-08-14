using SiteSearch.Core.Models;
using System.Threading;
using System.Threading.Tasks;

namespace SiteSearch.Core.Interfaces
{
    public interface ISearchIndex<T>
    {
        Task CreateIndexAsync(CancellationToken cancellationToken = default);
        Task<SearchResult<T>> SearchAsync(SearchQuery query, CancellationToken cancellationToken = default);
        SearchFieldInfo GetSearchFieldByAlias(string alias);
        IIngestionContext<T> StartUpdates();
    }
}
