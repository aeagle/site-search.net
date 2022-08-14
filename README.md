# SiteSearch.NET

_Note SiteSearch.NET is work in progress_

A simple full text search abstraction with an in-process Lucene.NET file-based implementation and .NET middleware allowing search interfaces to be quickly built.

- Ingestion
- Full text search
- Paging
- Faceting

## Why?

There a many full text search engines available. Popular Lucene based search engines for example are `Elastic` and `SOLR`. These usually require specific server setup or clusters which provide redundency and highly available instances. For very simple search UIs on simple websites can often be overkill.

This abstraction and the provided Lucene implementation aims to allow quick setup up of a basic file based searchable index of documents as an in-process search engine allowing it to be used on even the most basic website hosting.

Because all search functionality is abstracted a different server/cluster based search engine could be implemented and replace the file based Lucene implementation later without too much effort.

## Aims

- To provide an easy way to map query string values to search criteria
- Allow flexibility in ingestion of content either offline or online via background jobs
- Provide out of the box common paging and faceting / filtering functionality found on most search interfaces

## Basic usage

First we start with a class to represent searchable items:

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

#### App startup

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

Here we pass the root path of the file-based index that will be used to store the content.

Setup the search middleware:

```csharp
public void Configure(IApplicationBuilder app)
{
    ...

    app.UseSearch<SearchItem>("/search");

    ...
}
```

When requests are made to the path specified `/search`, SiteSearch.NET will automatically perform searches based on the criteria passed in the query string of the request and place the results in a search context.

#### Ingestion

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

#### Exposing the search interface

Using dependency injection, you can inject the `SearchContext`. This context contains everything you need to display a search UI for the current search result. Here we pass it directly to a view:

```csharp

public class HomeController : Controller
{
    private readonly SearchContext searchContext;

    public HomeController(
        SearchContext searchContext)
    {
        this.searchContext = searchContext ?? throw new ArgumentNullException(nameof(searchContext));
    }

    [Route("search")] // Matches the path configured on startup
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

The SiteSearch.Test project demonstrates a search page using the following dataset:

- https://www.kaggle.com/datasets/rmisra/news-category-dataset

You should download the dataset from Kaggle (requires registration) and place the archived json file `News_Category_Dataset_v2.json` in the src/SiteSearch.Test folder in the project before running the SiteSearch.Test project.
