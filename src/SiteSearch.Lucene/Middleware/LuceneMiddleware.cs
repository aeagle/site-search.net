using Microsoft.Extensions.DependencyInjection;
using SiteSearch.Core.Interfaces;
using SiteSearch.Core.Models;
using System;

namespace SiteSearch.Lucene.Middleware
{
    public static class LuceneMiddlewareExtensions
    {
        public static IServiceCollection AddLuceneSearch<T>(
            this IServiceCollection services,
            Action<LuceneSearchIndexOptions, IServiceProvider> optionsDescriptor = null) where T : class, new()
        {
            services.AddSingleton<ISearchIndex<T>>((ctx) => {

                var options = new LuceneSearchIndexOptions();

                if (optionsDescriptor != null)
                {
                    optionsDescriptor(options, ctx);
                }

                return new LuceneSearchIndex<T>(options);
            });


            services.AddScoped(ctx => new SearchContext());

            return services;
        }
    }
}
