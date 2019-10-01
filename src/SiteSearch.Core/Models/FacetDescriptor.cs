namespace SiteSearch.Core.Models
{
    public class FacetDescriptor
    {
        private readonly SearchQuery searchQuery;

        public FacetDescriptor(SearchQuery searchQuery)
        {
            this.searchQuery = searchQuery;
        }

        public FacetDescriptor Field(string field)
        {
            searchQuery.Facets.Add(field);
            return this;
        }
    }
}
