using System;

namespace SiteSearch.Test.Models
{
    public class NewsArticle
    {
        public DateTime Date { get; set; }
        public string Headline { get; set; }
        public string Short_Description { get; set; }
        public string Category { get; set; }
        public string Authors { get; set; }
        public string Link { get; set; }
    }
}
