using SiteSearch.Core.Models;
using System.ComponentModel;

public class SearchItem
{
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

    [Keyword, Store, TermFacet]
    [SearchAlias("c")]
    [DisplayName("News category")]
    public string Category { get; set; }

    [Keyword, Store]
    public string Url { get; set; }

    [SearchAlias("q")]
    public string Text => $"{Title} {Precis} {Body}".Trim();
}