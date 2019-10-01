using System;

namespace SiteSearch.Core.Models
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class IdAttribute : Attribute
    {
        public IdAttribute()
        {
        }
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class KeywordAttribute : Attribute
    {
        public KeywordAttribute()
        {
        }
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class StoreAttribute : Attribute
    {
        public StoreAttribute()
        {
        }
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class TermFacetAttribute : Attribute
    {
        public TermFacetAttribute()
        {
        }
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class RangeFacetAttribute : Attribute
    {
        public RangeFacetAttribute()
        {
        }
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class SearchAliasAttribute : Attribute
    {
        public string Alias { get; private set; }

        public SearchAliasAttribute(string alias)
        {
            Alias = alias ?? throw new ArgumentNullException(nameof(alias));
        }
    }
}
