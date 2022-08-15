using SiteSearch.Core.Extensions;
using SiteSearch.Core.Interfaces;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace SiteSearch.Core.Models
{
    public class SearchQuery<T>
    {
        public SearchCurrentCriteria<T> Criteria { get; private set; }

        public SearchQuery(ISearchIndex<T> searchIndex, NameValueCollection baseCriteria)
        {
            Criteria = new SearchCurrentCriteria<T>(searchIndex, baseCriteria);
            setupCriteria(this);
        }

        private void setupCriteria(SearchQuery<T> queryDefinition)
        {
            queryDefinition = queryDefinition.FacetOn(x => x.Field("category"));

            if (Criteria.Limit.HasValue)
            {
                queryDefinition = queryDefinition.Limit(Criteria.Limit.Value);
            }

            foreach (var field in Criteria.FieldCriteria)
            {
                queryDefinition =
                    queryDefinition.TermQuery(
                        field.Field,
                        field.Value
                    );
            }
        }

        public int Limit { get; set; } = 20;
        public int FacetMax { get; set; } = 0;
        public IList<(SearchFieldInfo field, string value)> TermQueries { get; set; } = new List<(SearchFieldInfo, string)>();
        public IList<string> Facets { get; set; } = new List<string>();
    }
}
