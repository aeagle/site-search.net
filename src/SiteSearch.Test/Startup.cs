using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SiteSearch.Core.Interfaces;
using SiteSearch.Lucene;
using SiteSearch.Middleware;

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
            services.AddLuceneSearch<SearchItem>((ctx) =>
            {
                var hostingEnvironment = ctx.GetRequiredService<IWebHostEnvironment>();
                return Path.Combine(hostingEnvironment.ContentRootPath, "search-index");
            });

            services.AddHostedService<SetupSearchIndex>(); 

            services.AddControllersWithViews().AddRazorRuntimeCompilation();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app, 
            IWebHostEnvironment env)
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

            app.UseSearch<SearchItem>("/search");

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }

    public class SetupSearchIndex : IHostedService
    {
        private readonly ISearchIndex<SearchItem> searchIndex;

        public SetupSearchIndex(ISearchIndex<SearchItem> searchIndex)
        {
            this.searchIndex = searchIndex ?? throw new ArgumentNullException(nameof(searchIndex));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await searchIndex.CreateIndexAsync();

            var testItem = new SearchItem
            {
                Id = "1234",
                Url = "https://www.google.com",
                Title = "This is a test item",
                Precis = "Lorem ipsum dolor sit amet consectetur adipisicing elit. Illum hic aut, ex voluptatibus quidem autem, ipsam expedita tempora nisi possimus nam laboriosam in voluptates consectetur eligendi reprehenderit quibusdam velit aspernatur.",
                Body = "Lorem ipsum dolor sit amet consectetur adipisicing elit. Illum hic aut, ex voluptatibus quidem autem, ipsam expedita tempora nisi possimus nam laboriosam in voluptates consectetur eligendi reprehenderit quibusdam velit aspernatur."
            };

            await searchIndex.IndexAsync(testItem);

            testItem = new SearchItem
            {
                Id = "4321",
                Url = "https://www.google.com",
                Title = "Red green yellow blue",
                Precis = "Lorem ipsum dolor sit amet consectetur adipisicing elit. Illum hic aut, ex voluptatibus quidem autem, ipsam expedita tempora nisi possimus nam laboriosam in voluptates consectetur eligendi reprehenderit quibusdam velit aspernatur.",
                Body = "Lorem ipsum dolor sit amet consectetur adipisicing elit. Illum hic aut, ex voluptatibus quidem autem, ipsam expedita tempora nisi possimus nam laboriosam in voluptates consectetur eligendi reprehenderit quibusdam velit aspernatur."
            };

            await searchIndex.IndexAsync(testItem);
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
