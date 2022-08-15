using System;
using System.Collections.Concurrent;

namespace SiteSearch.Core.Models
{
    public class SearchContext
    {
        private ConcurrentDictionary<Type, dynamic> results = new ConcurrentDictionary<Type, dynamic>();

        public void Set<T>(SearchResult<T> results)
        {
            this.results.AddOrUpdate(
                typeof(T), 
                results ?? throw new ArgumentNullException(nameof(results)),
                (key, existing) => results ?? throw new ArgumentNullException(nameof(results))
            );
        }

        public SearchResult<T> Get<T>()
        {
            if (results.TryGetValue(typeof(T), out var result))
            {
                return result as SearchResult<T>;
            }
            throw new InvalidOperationException($"No search results found for type {typeof(T).Name}");
        }
    }
}
