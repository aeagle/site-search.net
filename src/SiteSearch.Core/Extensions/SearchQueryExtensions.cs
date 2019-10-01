using SiteSearch.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SiteSearch.Core.Extensions
{
    public static class SearchQueryExtensions
    {
        public static SearchQuery TermQuery(this SearchQuery query, string field, string value)
        {
            query.TermQueries.Add((field, value));
            return query;
        }

        public static SearchQuery FacetOn(this SearchQuery query, Action<FacetDescriptor> descriptor, int max)
        {
            descriptor(new FacetDescriptor(query));
            query.FacetMax = max;
            return query;
        }

        public static SearchQuery Limit(this SearchQuery query, int limit)
        {
            query.Limit = limit;
            return query;
        }
    }
}
