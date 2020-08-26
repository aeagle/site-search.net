using SiteSearch.Core.Models;

public class SearchItem
{
    [Id]
    public string Id { get; set; }

    [Store]
    public string Title { get; set; }

    [Store]
    public string Precis { get; set; }

    [Store]
    public string Body { get; set; }

    [Keyword, Store]
    public string Url { get; set; }

    [SearchAlias("q")]
    public string Text => $"{Title} {Precis} {Body}".Trim();
}