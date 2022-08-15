using System;
using SiteSearch.Core.Models;
using SiteSearch.Middleware;

namespace SiteSearch.Core.Extensions
{
    public static class SearchMiddlewareOptionsExtensions
    {
        public static SearchMiddlewareOptions<T> FacetOn<T>(this SearchMiddlewareOptions<T> options, Action<FacetDescriptor<T>> descriptor)
        {
            descriptor(new FacetDescriptor<T>(options, options.searchIndex));
            return options;
        }
    }
}
