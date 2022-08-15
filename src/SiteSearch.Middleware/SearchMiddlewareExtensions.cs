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
            this IApplicationBuilder builder, string path, Action<SearchMiddlewareOptions<T>> optionsDescriptor = null) where T : class, new()
        {
            var options = new SearchMiddlewareOptions<T>(builder.ApplicationServices.GetRequiredService<ISearchIndex<T>>());
            if (optionsDescriptor != null)
            {
                optionsDescriptor(options);
            }

            return
                builder.UseWhen(
                    context => context.Request.Path.StartsWithSegments(path),
                    appBuilder => appBuilder.UseMiddleware<SearchMiddleware<T>>(options)
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
