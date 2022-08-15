using Microsoft.AspNetCore.Http;
using SiteSearch.Core.Extensions;
using SiteSearch.Core.Interfaces;
using SiteSearch.Core.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace SiteSearch.Middleware
{
    public class SearchMiddleware<T>
    {
        private readonly ISearchIndex<T> searchIndex;
        private readonly SearchMiddlewareOptions<T> options;
        private readonly RequestDelegate _next;

        public SearchMiddleware(
            RequestDelegate next, 
            ISearchIndex<T> searchIndex,
            SearchMiddlewareOptions<T> options)
        {
            this.searchIndex = searchIndex ?? throw new ArgumentNullException(nameof(searchIndex));
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, SearchContext searchContext)
        {
            // Extract criteria from query string
            var searchQuery = searchIndex.CreateSearchQuery(HttpUtility.ParseQueryString(context.Request.QueryString.Value));

            // Apply middleware options to queries
            if (options.FacetOn.Any())
            {
                foreach (var facet in options.FacetOn)
                {
                    searchQuery = searchQuery.FacetOn(facet.field, facet.maxFacets);
                }
            }

            // Do search
            var result = await searchIndex.SearchAsync(searchQuery);

            // Store result in search context
            searchContext.Set(result);

            // Call the next delegate/middleware in the pipeline
            await _next(context);
        }
    }
}
