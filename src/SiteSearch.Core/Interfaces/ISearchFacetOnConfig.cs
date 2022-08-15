using SiteSearch.Core.Models;
using System.Collections.Generic;

namespace SiteSearch.Core.Interfaces
{
    public interface ISearchFacetOnConfig
    {
        IList<(SearchFieldInfo field, int maxFacets)> FacetOn { get; }
    }
}
