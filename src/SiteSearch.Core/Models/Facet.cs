using SiteSearch.Core.Extensions;
using System;

namespace SiteSearch.Core.Models
{
    public class Facet
    {
        private readonly FacetGroup group;

        public Facet(FacetGroup group)
        {
            this.group = group ?? throw new ArgumentNullException(nameof(group));
        }

        public string Name => DisplayName ?? Key;
        public string Key { get; set; }
        public string DisplayName { get; set; }
        public long Count { get; set; }
        public string DrillDownUrl => $"?{group.currentCriteria.AddCriteria(group.fieldInfo.Alias, Key).AsQueryString()}";

        public override string ToString()
        {
            return $"{Key} ({Count})";
        }
    }
}
