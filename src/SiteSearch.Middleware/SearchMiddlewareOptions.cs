using SiteSearch.Core.Interfaces;
using SiteSearch.Core.Models;
using System;
using System.Collections.Generic;

namespace SiteSearch.Middleware
{
    public class SearchMiddlewareOptions<T> : ISearchFacetOnConfig
    {
        protected readonly internal ISearchIndex<T> searchIndex;

        public SearchMiddlewareOptions(ISearchIndex<T> searchIndex)
        {
            this.searchIndex = searchIndex ?? throw new ArgumentNullException(nameof(searchIndex));
        }

        public IList<(SearchFieldInfo field, int maxFacets)> FacetOn { get; set; } = new List<(SearchFieldInfo field, int maxFacets)>();
    }
}
