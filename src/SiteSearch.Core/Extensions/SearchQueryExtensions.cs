using System;
using SiteSearch.Core.Models;

namespace SiteSearch.Core.Extensions
{
    public static class SearchQueryExtensions
    {
        public static SearchQuery<T> TermQuery<T>(this SearchQuery<T> query, SearchFieldInfo field, string value)
        {
            query.TermQueries.Add((field, value));
            return query;
        }

        public static SearchQuery<T> FacetOn<T>(this SearchQuery<T> query, Action<FacetDescriptor<T>> descriptor, int max = 100)
        {
            descriptor(new FacetDescriptor<T>(query));
            query.FacetMax = max;
            return query;
        }

        public static SearchQuery<T> Limit<T>(this SearchQuery<T> query, int limit)
        {
            query.Limit = limit;
            return query;
        }
    }
}
