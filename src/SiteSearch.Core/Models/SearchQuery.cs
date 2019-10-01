using System.Collections.Generic;

namespace SiteSearch.Core.Models
{
    public class SearchQuery
    {
        public int Limit { get; set; } = 20;
        public int FacetMax { get; set; } = 0;
        public IList<(string field, string value)> TermQueries { get; set; } = new List<(string, string)>();
        public IList<string> Facets { get; set; } = new List<string>();
    }
}
