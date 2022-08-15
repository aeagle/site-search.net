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

        public static SearchQuery<T> FacetOn<T>(this SearchQuery<T> query, Action<FacetDescriptor<T>> descriptor)
        {
            descriptor(new FacetDescriptor<T>(query, query.searchIndex));
            return query;
        }

        public static SearchQuery<T> FacetOn<T>(this SearchQuery<T> query, SearchFieldInfo field, int maxFacets = 20)
        {
            query.FacetOn.Add((field, maxFacets));
            return query;
        }

        public static SearchQuery<T> Limit<T>(this SearchQuery<T> query, int limit)
        {
            query.Limit = limit;
            return query;
        }
    }
}
