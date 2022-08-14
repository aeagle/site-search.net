using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using SiteSearch.Core.Interfaces;
using SiteSearch.Core.Models;
using SiteSearch.Lucene;
using System;

namespace SiteSearch.Middleware
{
    public static class SearchMiddlewareExtensions
    {
        public static IApplicationBuilder UseSearch<T>(
            this IApplicationBuilder builder, string path) where T : class, new()
        {
            return
                builder.UseWhen(
                    context => context.Request.Path.StartsWithSegments(path),
                    appBuilder => appBuilder.UseMiddleware<SearchMiddleware<T>>()
                );
        }

        public static IServiceCollection AddLuceneSearch<T>(
            this IServiceCollection services, Func<IServiceProvider, string> indexPath) where T : class, new()
        {
            services.AddSingleton<ISearchIndex<T>>((ctx) => {
                return new LuceneSearchIndex<T>(
                    new LuceneSearchIndexOptions
                    {
                        IndexPath = indexPath(ctx)
                    }
                );
            });

            services.AddScoped(ctx => new SearchContext());

            return services;
        }
    }
}
