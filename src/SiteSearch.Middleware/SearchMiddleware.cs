using Microsoft.AspNetCore.Builder;
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

        public SearchMiddleware(RequestDelegate next, ISearchIndex<T> searchIndex)
        {
            this.searchIndex = searchIndex ?? throw new ArgumentNullException(nameof(searchIndex));
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/search"))
            {
                var queryDefinition =
                    new SearchQuery();
                        // .FacetOn(f => f
                        //     .Field("contenttype"),
                        //     max: 10
                        // );

                string limit;
                if (!string.IsNullOrWhiteSpace(limit = context.Request.Query["ps"]))
                {
                    if (int.TryParse(limit, out var limitVal))
                    {
                        queryDefinition = queryDefinition.Limit(limitVal);
                    }
                }

                string query;
                if (!string.IsNullOrWhiteSpace(query = context.Request.Query["q"]))
                {
                    queryDefinition = queryDefinition.TermQuery("text", query);
                }

                var result = searchIndex.Search(queryDefinition);
                context.Items["_search_result"] = result;
            }

            // Call the next delegate/middleware in the pipeline
            await _next(context);
        }
    }

    public static class SearchMiddlewareExtensions
    {
        public static IApplicationBuilder UseSearch<T>(
            this IApplicationBuilder builder) where T : class
        {
            return builder.UseMiddleware<SearchMiddleware<T>>();
        }
    }
}
