# SiteSearch.NET

_Note SiteSearch.NET is work in progress_

A simple full text search abstraction with an in-process Lucene.NET file-based implementation and .NET middleware allowing search interfaces to be quickly built.

- Ingestion
- Full text search
- Sorting
- Paging
- Faceting

## Why?

There a many full text search engines available. Popular Lucene based search engines for example are `Elastic` and `SOLR`. These usually require server clusters which provide redundency and highly available instances. For very simple search UIs on simple websites, this can often be overkill.

This abstraction:

1) Simplifies setting up a search interface allowing common actions such as full text searches, paging, sorting, faceting.
2) Provides a file-based Lucene implementation that allows quick searchable index of documents as an in-process search engine allowing it to be used on even the most basic website hosting.

Also, because all search functionality is abstracted a different implementation could be implemented (i.e. Elastic) later without affecting consuming code.

## Aims

- Provide out of the box common sorting, paging and faceting functionality found on most search interfaces
- To provide middleware that eases driving a search interface
- Allow flexibility in ingestion of content either offline or online via background jobs

## Basic usage

First we start with a class to represent searchable items:

```csharp
public class SearchItem {
    [Id]
    [SearchAlias("id")]
    public string Id { get; set; }

    [Store]
    [SearchAlias("d")]
    public DateTime PublicationDate { get; set; }

    [Store]
    [SearchAlias("t")]
    public string Title { get; set; }

    [Store]
    [SearchAlias("p")]
    public string Precis { get; set; }

    [Keyword, Store, TermFacet]
    [SearchAlias("c")]
    [DisplayName("News category")]
    public string Category { get; set; }

    [Keyword, Store]
    public string Url { get; set; }

    [SearchAlias("q")]
    public string Text => $"{Title} {Precis} {Body}".Trim();

    [Keyword, TermFacet]
    [SearchAlias("pd")]
    [DisplayName("Period")]
    public string MonthYear => PublicationDate.ToString("MMMM yyyy");
}
```

#### App startup

Setup the search services:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    ...

    string getRootIndexPath(IWebHostEnvironment hostingEnvironment) =>
        Path.Combine(hostingEnvironment.ContentRootPath, "search-index");

    services.AddLuceneSearch<SearchItem>((opts, ctx) => opts
        .IndexPath(getRootIndexPath(ctx.GetRequiredService<IWebHostEnvironment>()))
    );

    ...
}
```

Here we pass the root path of the file-based index that will be used to store the content.

Setup the search middleware:

```csharp
public void Configure(IApplicationBuilder app)
{
    ...

    app.UseSearch<SearchItem>(
        "/search",
        opts => opts
            .FacetOn(x => x.Field(f => f.Category), maxFacets: 50)
            .FacetOn(x => x.Field(f => f.MonthYear), maxFacets: 50)
    );

    ...
}
```

When requests are made to the path specified `/search`, SiteSearch.NET will automatically perform searches based on the criteria passed in the query string of the request and place the results in a search context. Options passed here allow defaults to be applied to all searches.

#### Ingestion

Ensure the search index exists and ingest some content:

```csharp
await searchIndex.CreateIndexAsync();

using (var context = searchIndex.StartUpdates())
{
    await context.IndexAsync(new SearchItem {
        Id = "1234",
        PublicationDate = new DateTime(2022, 1, 1),
        Category = "Category 1",
        Url = "http://www.google.com/",
        Title = "This is a indexed search item",
        Precis = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed sit amet tincidunt magna, sed consequat lorem. Integer sit amet sollicitudin lorem, id luctus magna. Phasellus dapibus tellus magna, id porta velit fermentum non."
    });
    await context.IndexAsync(new SearchItem {
        Id = "4321",
        PublicationDate = new DateTime(2020, 1, 1),
        Category = "Category 2",
        Url = "http://www.microsoft.com/",
        Title = "This is another indexed search item",
        Precis = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed sit amet tincidunt magna, sed consequat lorem. Integer sit amet sollicitudin lorem, id luctus magna. Phasellus dapibus tellus magna, id porta velit fermentum non."
    });
}

```

#### Exposing the search interface

Using dependency injection, you can inject the `SearchContext`. This context contains everything you need to display a search UI for the current search result. Here using MVC we pass it directly to a Razor view from the controller:

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

```html
<div id="search-results">
  <form>
    <input type="text" autofocus name="q" value="@Model.CurrentCriteria.Term" />
    <button type="submit">Search</button>
  </form>

  <div class="search-facets">
    @if (Model.CurrentCriteria.FieldCriteria.Any()) {
    <h2>Applied criteria</h2>
    <ul>
      @foreach (var criteria in Model.CurrentCriteria.FieldCriteria) {
      <li>
        @(criteria.Name): @criteria.Value <a href="@criteria.RemoveUrl">X</a>
      </li>
      }
    </ul>
    } @foreach (var facetGroup in Model.FacetGroups) {
    <h2>@facetGroup.DisplayName</h2>
    <ul>
      @foreach (var facet in facetGroup.Facets) {
      <li><a href="@facet.DrillDownUrl">@facet.Name</a> (@facet.Count)</li>
      }
    </ul>
    }
  </div>

  @if (Model.Hits.Any()) {
  <p>Showing @Model.Hits.Count() of @Model.TotalHits results ...</p>

  <article class="result-list">
    @foreach (var item in Model.Hits) {
    <h2>
      <a href="@item.Url">@Html.Raw(item.Title)</a>
    </h2>
    <p>@Html.Raw(item.Precis)</p>
    }
  </article>
  } else {
  <p>No results found.</p>
  }
</div>
```

## To do

- Sorting
- Paging
- Facet sorting
- Range faceting
- Custom analysers
- Elastic implementation to check validity of abstraction
- Example API driven React app

## Test news article dataset

The SiteSearch.Test project demonstrates a search page using the following dataset:

- https://www.kaggle.com/datasets/rmisra/news-category-dataset

You should download the dataset from Kaggle (requires registration) and place the archived json file `News_Category_Dataset_v2.json` in the `src/SiteSearch.Test` folder in the project before running the `SiteSearch.Test` project.
