﻿using System.Reflection;

namespace SiteSearch.Core.Models
{
    public class SearchFieldInfo
    {
        public PropertyInfo PropertyInfo { get; set; }
        public bool Id { get; set; }
        public string Name => PropertyInfo.Name;
        public bool Keyword { get; set; }
        public bool Store { get; set; }
        public bool Facet { get; set; }
        public string Alias { get; set; }
        public string DisplayName { get; set; }
    }
}
