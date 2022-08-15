using System;
using System.Collections.Generic;

namespace SiteSearch.Core.Models
{
    public class SearchMetaData
    {
        public IDictionary<string, SearchFieldInfo> Fields = 
            new Dictionary<string, SearchFieldInfo>(StringComparer.OrdinalIgnoreCase);
    }
}
