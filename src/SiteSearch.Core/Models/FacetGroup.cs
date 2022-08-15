using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace SiteSearch.Core.Models
{
    public class FacetGroup
    {
        protected internal readonly SearchFieldInfo fieldInfo;
        protected internal readonly NameValueCollection currentCriteria;

        public FacetGroup(SearchFieldInfo fieldInfo, NameValueCollection currentCriteria)
        {
            this.fieldInfo = fieldInfo ?? throw new ArgumentNullException(nameof(fieldInfo));
            this.currentCriteria = currentCriteria ?? throw new ArgumentNullException(nameof(currentCriteria));
        }

        public string Field => fieldInfo.Name;
        public string DisplayName => fieldInfo.DisplayName ?? Field;
        public IList<Facet> Facets { get; set; } = new List<Facet>();

        public override string ToString()
        {
            return $"{DisplayName}";
        }
    }
}
