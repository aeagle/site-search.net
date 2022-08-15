using Microsoft.AspNetCore.Http;
using SiteSearch.Core.Interfaces;
using SiteSearch.Core.Models;
using System;
using System.Threading.Tasks;
using System.Web;

namespace SiteSearch.Middleware
{
    public class SearchMiddleware<T>
    {
        private readonly ISearchIndex<T> searchIndex;
        private readonly RequestDelegate _next;

        public SearchMiddleware(
            RequestDelegate next, 
            ISearchIndex<T> searchIndex)
        {
            this.searchIndex = searchIndex ?? throw new ArgumentNullException(nameof(searchIndex));
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, SearchContext searchContext)
        {
            // Extract criteria from query string
            var currentCriteria = 
                new SearchCurrentCriteria<T>(
                    searchIndex, 
                    HttpUtility.ParseQueryString(context.Request.QueryString.Value)
                );
            var searchQuery = currentCriteria.GetSearchQuery();

            // Do search
            var result = await searchIndex.SearchAsync(searchQuery);
            result.CurrentCriteria = currentCriteria;

            // Store result in search context
            searchContext.Set(result);

            // Call the next delegate/middleware in the pipeline
            await _next(context);
        }
    }
}
