using SiteSearch.Core.Extensions;
using SiteSearch.Core.Interfaces;
using System;
using System.Linq.Expressions;

namespace SiteSearch.Core.Models
{
    public class FacetDescriptor<T>
    {
        private readonly ISearchIndex<T> searchIndex;
        private readonly ISearchFacetOnConfig config;

        public FacetDescriptor(ISearchFacetOnConfig config, ISearchIndex<T> searchIndex)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
            this.searchIndex = searchIndex ?? throw new ArgumentNullException(nameof(searchIndex));
        }

        public FacetDescriptor<T> Field(Expression<Func<T, object>> field, int maxFacets = 20)
        {
            config.FacetOn.Add((searchIndex.GetSearchFieldByName(field.GetPropertyInfo().Name), maxFacets));
            return this;
        }
    }
}
