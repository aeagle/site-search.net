using SiteSearch.Core.Models;
using SiteSearch.Core.Extensions;
using System.Reflection;

namespace SiteSearch.Core.Utils
{
    public static class SearchMetaDataUtility
    {
        public static SearchMetaData GetMetaData<T>() where T : class
        {
            var result = new SearchMetaData();

            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties)
            {
                if (property.CanRead && !property.IsPropertyACollection())
                {
                    result.Fields.Add(
                        property.Name.ToLower(),
                        new SearchFieldInfo
                        {
                            PropertyInfo = property,
                            Id = property.HasAttribute<IdAttribute>(),
                            Keyword = property.HasAttribute<KeywordAttribute>(),
                            Store = property.HasAttribute<StoreAttribute>(),
                            Facet = property.HasAttribute<TermFacetAttribute>()
                        }
                    );
                }
            }

            return result;
        }
    }
}
