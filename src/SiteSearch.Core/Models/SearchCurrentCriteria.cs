﻿using SiteSearch.Core.Extensions;
using SiteSearch.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace SiteSearch.Core.Models
{
    public class SearchCurrentCriteria<T>
    {
        public NameValueCollection Criteria { get; private set; }

        public SearchCurrentCriteria(ISearchIndex<T> searchIndex, NameValueCollection criteria)
        {
            Criteria = criteria ?? throw new ArgumentNullException(nameof(criteria));

            Limit = criteria["ps"]?.ParseInt() ?? 10;

            foreach (string key in criteria.Keys)
            {
                var val = criteria[key];
                if (!string.IsNullOrEmpty(val))
                {
                    var field = searchIndex.GetSearchFieldByAlias(key);
                    if (field != null)
                    {
                        FieldCriteria.Add(new SearchFieldCriteria { Field = field, Value = val });
                    }
                }
            }
        }

        public int? Limit { get; set; }
        public IList<SearchFieldCriteria> FieldCriteria { get; set; } = new List<SearchFieldCriteria>();

        public string GetCriteriaValueByAlias(string alias) =>
            FieldCriteria.FirstOrDefault(x => x.Field.Alias == alias)?.Value;

        public class SearchFieldCriteria
        {
            public SearchFieldInfo Field { get; set; }
            public string Value { get; set; }
        }
    }
}
