namespace SiteSearch.Lucene.Extensions
{
    public static class LuceneSearchIndexOptionsExtensions
    {
        public static LuceneSearchIndexOptions IndexPath(this LuceneSearchIndexOptions options, string path)
        {
            options.IndexPath = path;
            return options;
        }
    }
}
