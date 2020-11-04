using SiteSearch.Core.Interfaces;
using SiteSearch.Core.Serialization;

namespace SiteSearch.Lucene
{
    public class LuceneSearchIndexOptions
    {
        public ISourceSerializer Serializer { get; set; } = new JSONNetSerializer();
        public string IndexPath { get; set; }
    }
}
