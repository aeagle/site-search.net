using System.Collections.Generic;
using System.Linq;

namespace SiteSearch.Core.Models
{
    public class SearchResult<T>
    {
        public SearchCurrentCriteria CurrentCriteria { get; set; }
        public int TotalHits { get; set; }
        public IEnumerable<T> Hits { get; set; } = Enumerable.Empty<T>();
        public IList<FacetGroup> FacetGroups { get; set; } = new List<FacetGroup>();
    }

    public class SearchCurrentCriteria
    {
        public int? Limit { get; set; }
        public IList<SearchFieldCriteria> FieldCriteria { get; set; } = new List<SearchFieldCriteria>();

        public string GetCriteriaValueByAlias(string alias) =>
            FieldCriteria.FirstOrDefault(x => x.Field.Alias == alias)?.Value;
    }

    public class SearchFieldCriteria
    { 
        public SearchFieldInfo Field { get; set; }
        public string Value { get; set; }
    }
}
