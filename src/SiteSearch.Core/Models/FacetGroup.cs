using System.Collections.Generic;

namespace SiteSearch.Core.Models
{
    public class FacetGroup
    {
        public string Field { get; set; }
        public IList<Facet> Facets { get; set; } = new List<Facet>();

        public override string ToString()
        {
            return $"{Field}";
        }
    }
}
