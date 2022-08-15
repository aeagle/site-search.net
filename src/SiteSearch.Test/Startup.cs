using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using SiteSearch.Core.Interfaces;
using SiteSearch.Middleware;
using SiteSearch.Test.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SiteSearch.Core.Extensions;

namespace SiteSearch.Test
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            string getRootIndexPath(IWebHostEnvironment hostingEnvironment) =>
                Path.Combine(hostingEnvironment.ContentRootPath, "search-index");

            services.AddLuceneSearch<SearchItem>((ctx) => getRootIndexPath(ctx.GetRequiredService<IWebHostEnvironment>()));
            services.AddHostedService<CreateSearchIndex>();
            services.AddHostedService((ctx) => 
                new SetupSearchIndex(
                    ctx.GetRequiredService<ISearchIndex<SearchItem>>(),
                    ctx.GetRequiredService<IWebHostEnvironment>().ContentRootPath
                )
            );

            services.AddControllersWithViews().AddRazorRuntimeCompilation();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {   
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
                app.UseHttpsRedirection();
            }
            
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseSearch<SearchItem>(
                "/search",
                opts => opts
                    .FacetOn(x => x.Field(f => f.Category))
                    .FacetOn(x => x.Field(f => f.MonthYear))
            );

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }

    public class CreateSearchIndex : IHostedService
    {
        private readonly ISearchIndex<SearchItem> searchIndex;

        public CreateSearchIndex(ISearchIndex<SearchItem> searchIndex)
        {
            this.searchIndex = searchIndex ?? throw new ArgumentNullException(nameof(searchIndex));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await searchIndex.CreateIndexAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    public class SetupSearchIndex : BackgroundService
    {
        private readonly ISearchIndex<SearchItem> searchIndex;
        private readonly string appRootPath;

        public SetupSearchIndex(ISearchIndex<SearchItem> searchIndex, string appRootPath)
        {
            this.searchIndex = searchIndex ?? throw new ArgumentNullException(nameof(searchIndex));
            this.appRootPath = appRootPath ?? throw new ArgumentNullException(nameof(appRootPath));
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            static string hash(string input)
            {
                using (var sha1 = SHA1.Create())
                {
                    var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
                    var sb = new StringBuilder(hash.Length * 2);

                    foreach (byte b in hash)
                    {
                        // can be "x2" if you want lowercase
                        sb.Append(b.ToString("X2"));
                    }

                    return sb.ToString();
                }
            }

            using (var context = searchIndex.StartUpdates())
            {
                using (var stream = File.OpenRead($@"{appRootPath}\News_Category_Dataset_v2.json"))
                using (var sr = new StreamReader(stream))
                {
                    var items = JsonExtensions.WalkObjects<NewsArticle>(sr);
                    var processed = 0;
                    foreach (var item in items)
                    {
                        var testItem = new SearchItem
                        {
                            Id = hash($"{item.Headline} {item.Link}"),
                            PublicationDate = item.Date,
                            Url = item.Link,
                            Title = item.Headline,
                            Category = item.Category,
                            Precis = item.Short_Description
                        };

                        await context.IndexAsync(testItem);
                        processed++;

                        if (processed == 10000)
                        {
                            break;
                        }
                        if (cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }
                    }
                }
            }
        }
    }

    public static class JsonExtensions
    {
        public static IEnumerable<T> WalkObjects<T>(StreamReader textReader)
        {
            string line = null;
            do
            {
                line = textReader.ReadLine();
                if (line != null)
                {
                    yield return JsonConvert.DeserializeObject<T>(line);
                }
            } while (line != null);
        }
    }
}
