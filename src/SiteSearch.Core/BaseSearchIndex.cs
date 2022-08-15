using SiteSearch.Core.Models;
using SiteSearch.Core.Utils;
using System.Collections.Generic;
using System.Linq;

namespace SiteSearch.Core
{
    public class BaseSearchIndex<T> where T : class, new()
    {
        protected readonly SearchMetaData searchMetaData;
        protected readonly Dictionary<string, SearchFieldInfo> searchAliases;

        public BaseSearchIndex()
        {
            searchMetaData = SearchMetaDataUtility.GetMetaData<T>();
            searchAliases =
                searchMetaData.Fields
                    .Where(x => !string.IsNullOrEmpty(x.Value.Alias))
                    .ToDictionary(key => key.Value.Alias, val => val.Value);
        }

        public SearchFieldInfo GetSearchFieldByAlias(string alias)
        {
            return searchAliases.TryGetValue(alias, out var field) ? field : null;
        }

        public SearchFieldInfo GetSearchFieldByName(string fieldName)
        {
            return searchMetaData.Fields.TryGetValue(fieldName, out var field) ? field : null;
        }
    }
}
