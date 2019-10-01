using SiteSearch.Core.Models;
using System.Collections.Generic;

namespace SiteSearch.Core.Interfaces
{
    public interface ISearchIndex<T>
    {
        void CreateIndex();
        void Index(T document);
        void Index(IEnumerable<T> documents);
        SearchResult<T> Search(SearchQuery query);
    }
}
