using Lucene.Net.Analysis;
using Lucene.Net.Util;
using Nito.AsyncEx;
using SiteSearch.Lucene.Analyzers;

namespace SiteSearch.Lucene
{
    public class LuceneIndex
    {
        private static AsyncReaderWriterLock writerLock = new AsyncReaderWriterLock();

        public LuceneVersion MATCH_LUCENE_VERSION => LuceneVersion.LUCENE_48;
        public AsyncReaderWriterLock WriterLock => writerLock;
        public Analyzer SetupAnalyzer() => new EnglishSearchAnalyzer(MATCH_LUCENE_VERSION);
    }
}
