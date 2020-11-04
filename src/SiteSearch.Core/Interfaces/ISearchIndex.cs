using SiteSearch.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SiteSearch.Core.Interfaces
{
    public interface ISearchIndex<T>
    {
        Task CreateIndexAsync();
        Task IndexAsync(T document);
        Task IndexAsync(IEnumerable<T> documents);
        Task<SearchResult<T>> SearchAsync(SearchQuery query);
    }
}
