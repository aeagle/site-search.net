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
    }
}
