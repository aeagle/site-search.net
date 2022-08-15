using SiteSearch.Core.Extensions;
using SiteSearch.Core.Interfaces;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace SiteSearch.Core.Models
{
    public class SearchResult<T>
    {
        public SearchCurrentCriteria<T> CurrentCriteria { get; set; }
        public int TotalHits { get; set; }
        public IEnumerable<T> Hits { get; set; } = Enumerable.Empty<T>();
        public IList<FacetGroup> FacetGroups { get; set; } = new List<FacetGroup>();
    }

    public class SearchCurrentCriteria<T>
    {
        public SearchCurrentCriteria(ISearchIndex<T> searchIndex, NameValueCollection criteria)
        {
            Limit = criteria["ps"]?.ParseInt() ?? 10;

            foreach (string key in criteria.Keys)
            {
                var val = criteria[key];
                if (!string.IsNullOrEmpty(val))
                {
                    var field = searchIndex.GetSearchFieldByAlias(key);
                    if (field != null)
                    {
                        FieldCriteria.Add(new SearchFieldCriteria { Field = field, Value = val });
                    }
                }
            }
        }

        public int? Limit { get; set; }
        public IList<SearchFieldCriteria> FieldCriteria { get; set; } = new List<SearchFieldCriteria>();

        public SearchQuery GetSearchQuery()
        {
            var queryDefinition = new SearchQuery();
            queryDefinition.FacetOn(x => x.Field("category"), 100);

            if (Limit.HasValue)
            {
                queryDefinition = queryDefinition.Limit(Limit.Value);
            }

            foreach (var field in FieldCriteria)
            {
                queryDefinition =
                    queryDefinition.TermQuery(
                        field.Field,
                        field.Value
                    );
            }

            return queryDefinition;
        }

        public string GetCriteriaValueByAlias(string alias) =>
            FieldCriteria.FirstOrDefault(x => x.Field.Alias == alias)?.Value;
    }

    public class SearchFieldCriteria
    { 
        public SearchFieldInfo Field { get; set; }
        public string Value { get; set; }
    }
}
