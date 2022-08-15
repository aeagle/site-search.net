using SiteSearch.Core.Extensions;
using System;
using System.Linq.Expressions;

namespace SiteSearch.Core.Models
{
    public class FacetDescriptor<T>
    {
        private readonly SearchQuery<T> searchQuery;

        public FacetDescriptor(SearchQuery<T> searchQuery)
        {
            this.searchQuery = searchQuery;
        }

        public FacetDescriptor<T> Field(string field)
        {
            searchQuery.Facets.Add(field);
            return this;
        }

        public FacetDescriptor<T> Field(Expression<Func<T, object>> field)
        {
            searchQuery.Facets.Add(field.GetPropertyInfo().Name);
            return this;
        }
    }
}
