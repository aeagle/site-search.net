using System;

namespace SiteSearch.Core.Models
{
    public class SearchContext
    {
        private dynamic results { get; set; }

        public void Set<T>(SearchResult<T> results)
        {
            this.results = results ?? throw new ArgumentNullException(nameof(results));
        }

        public SearchResult<T> Get<T>()
        {
            return results as SearchResult<T>;
        }
    }
}
