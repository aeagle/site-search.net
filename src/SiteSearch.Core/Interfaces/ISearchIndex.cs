using SiteSearch.Core.Models;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;

namespace SiteSearch.Core.Interfaces
{
    public interface ISearchIndex<T>
    {
        Task CreateIndexAsync(CancellationToken cancellationToken = default);
        Task<SearchResult<T>> SearchAsync(SearchQuery<T> query, CancellationToken cancellationToken = default);
        SearchFieldInfo GetSearchFieldByAlias(string alias);
        SearchQuery<T> CreateSearchQuery(NameValueCollection criteria);
        IIngestionContext<T> StartUpdates();
    }
}
