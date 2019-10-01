using System.Collections.Generic;
using System.Linq;

namespace SiteSearch.Core.Models
{
    public class SearchResult<T>
    {
        public int TotalHits { get; set; }
        public IEnumerable<T> Hits { get; set; } = Enumerable.Empty<T>();
        public IList<FacetGroup> FacetGroups { get; set; } = new List<FacetGroup>();
    }
}
