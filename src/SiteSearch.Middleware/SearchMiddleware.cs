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
                    Limit = context.Request.Query["ps"].ParseInt()
                };

            var queryDefinition = new SearchQuery();

            if (currentCriteria.Limit.HasValue)
            {
                queryDefinition = queryDefinition.Limit(currentCriteria.Limit.Value);
            }

            foreach (var criteria in context.Request.Query)
            {
                var val = context.Request.Query[criteria.Key];
                if (!string.IsNullOrEmpty(val))
                {
                    var field = searchIndex.GetSearchFieldByAlias(criteria.Key);
                    if (field != null)
                    {
                        currentCriteria.FieldCriteria.Add(new SearchFieldCriteria { Field = field, Value = val });

                        queryDefinition =
                            queryDefinition.TermQuery(
                                field.PropertyInfo.Name.ToLower(),
                                val
                            );
                    }
                }
            }

            // Do search
            var result = await searchIndex.SearchAsync(queryDefinition);
            result.CurrentCriteria = currentCriteria;

            // Store result in search context
            searchContext.Set(result);

            // Call the next delegate/middleware in the pipeline
            await _next(context);
        }
    }
}
