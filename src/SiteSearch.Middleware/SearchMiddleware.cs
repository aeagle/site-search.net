using Microsoft.AspNetCore.Http;
using SiteSearch.Core.Extensions;
using SiteSearch.Core.Interfaces;
using SiteSearch.Core.Models;
using System;
using System.Threading.Tasks;

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
                new SearchCurrentCriteria
                {
                    Term = context.Request.Query["q"],
                    Limit = context.Request.Query["ps"].ParseInt()
                };

            var queryDefinition = new SearchQuery();

            if (currentCriteria.Limit.HasValue)
            {
                queryDefinition = queryDefinition.Limit(currentCriteria.Limit.Value);
            }

            if (!string.IsNullOrEmpty(currentCriteria.Term))
            {
                queryDefinition = queryDefinition.TermQuery("text", currentCriteria.Term);
            }

            // Do search
            var result = await searchIndex.SearchAsync(queryDefinition);
            result.CurrentCriteria = currentCriteria;

            searchContext.Set(result);

            // Call the next delegate/middleware in the pipeline
            await _next(context);
        }
    }
}
