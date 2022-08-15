using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using SiteSearch.Core.Interfaces;
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
    }
}
