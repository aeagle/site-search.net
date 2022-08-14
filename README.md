# SiteSearch.NET

A simple full text search abstraction with a Lucene.NET file-based implementation and .NET middleware allowing search interfaces to be quickly built.

- Ingestion
- Full text search
- Paging
- Faceting

## Usage

Given a class to represent searchable items:

```csharp
public class SearchItem {
    [Id]
    [SearchAlias("id")]
    public string Id { get; set; }

    [Store]
    [SearchAlias("t")]
    public string Title { get; set; }

    [Store]
    [SearchAlias("p")]
    public string Precis { get; set; }

    [Store]
    [SearchAlias("b")]
    public string Body { get; set; }

    [Keyword, Store]
    public string Url { get; set; }

    [SearchAlias("q")]
    public string Text => $"{Title} {Precis} {Body}".Trim();
}
```

Setup the search services:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    ...

    services.AddLuceneSearch<SearchItem>((ctx) =>
    {
        var hostingEnvironment = ctx.GetRequiredService<IWebHostEnvironment>();
        return Path.Combine(hostingEnvironment.ContentRootPath, "search-index");
    });

    ...
}
```

Setup the search middleware:

```csharp
public void Configure(IApplicationBuilder app)
{
    ...

    app.UseSearch<SearchItem>("/search");

    ...
}
```

Ensure the search index exists and ingest some content:

```csharp
await searchIndex.CreateIndexAsync();
await searchIndex.IndexAsync(new SearchItem {
    Id = "1234",
    Url = "http://www.google.com/",
    Title = "This is a indexed search item",
    Precis = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed sit amet tincidunt magna, sed consequat lorem. Integer sit amet sollicitudin lorem, id luctus magna. Phasellus dapibus tellus magna, id porta velit fermentum non."
});

```

Pull the search context from your 'search controller action' and pass it to a view:

```csharp

public class HomeController : Controller
{
    private readonly SearchContext searchContext;

    public HomeController(
        SearchContext searchContext)
    {
        this.searchContext = searchContext ?? throw new ArgumentNullException(nameof(searchContext));
    }

    [Route("search")]
    public IActionResult Search()
    {
        return View(searchContext.Get<SearchItem>());
    }
}

```

Render the search results and related information:

```razor
<div id="search-results">
    <form>
        <input type="text" autofocus name="q" value="@Model.CurrentCriteria.Term" />
        <button type="submit">Search</button>
    </form>

    @if (Model.Hits.Any())
    {
        <p>
            Showing @Model.Hits.Count() of @Model.TotalHits results ...
        </p>

        <article class="result-list">
            @foreach (var item in Model.Hits)
            {
                <h2>
                    <a href="@item.Url">@Html.Raw(item.Title)</a>
                </h2>
                <p>@Html.Raw(item.Precis)</p>
            }
        </article>
    }
    else
    {
        <p>No results found.</p>
    }
</div>
```

## Test news article dataset

The SiteSearch.Test projects demonstrates a search page using the following dataset:

- https://www.kaggle.com/datasets/rmisra/news-category-dataset

You should download the dataset from Kaggle (requires registration) and place the archived json file `News_Category_Dataset_v2.json` in the src/SiteSearch.Test folder in the project before running the SiteSearch.Test project.
