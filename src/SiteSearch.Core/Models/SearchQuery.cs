using SiteSearch.Core.Extensions;
using SiteSearch.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace SiteSearch.Core.Models
{
    public class SearchQuery<T> : ISearchFacetOnConfig
    {
        protected readonly internal ISearchIndex<T> searchIndex;

        public SearchCurrentCriteria<T> Criteria { get; private set; }

        public SearchQuery(ISearchIndex<T> searchIndex, NameValueCollection baseCriteria)
        {
            this.searchIndex = searchIndex ?? throw new ArgumentNullException(nameof(searchIndex));

            Criteria = new SearchCurrentCriteria<T>(searchIndex, baseCriteria);
            setupCriteria(this);
        }

        private void setupCriteria(SearchQuery<T> queryDefinition)
        {
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

        public IList<(SearchFieldInfo field, string value)> TermQueries { get; set; } = new List<(SearchFieldInfo, string)>();
        public IList<(SearchFieldInfo field, int maxFacets)> FacetOn { get; set; } = new List<(SearchFieldInfo field, int maxFacets)>();
    }
}
