using Lucene.Net.Analysis;
using Lucene.Net.Facet;
using Lucene.Net.Facet.Taxonomy.Directory;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Nito.AsyncEx;
using SiteSearch.Lucene.Analyzers;
using System;
using System.IO;

namespace SiteSearch.Lucene
{
    public class LuceneIndex
    {
        private static AsyncReaderWriterLock writerLock = new AsyncReaderWriterLock();

        public LuceneVersion MATCH_LUCENE_VERSION => LuceneVersion.LUCENE_48;
        public AsyncReaderWriterLock WriterLock => writerLock;
        public Analyzer SetupAnalyzer() => new EnglishSearchAnalyzer(MATCH_LUCENE_VERSION);

        public readonly string indexPath;
        public readonly string indexType;

        public LuceneIndex(string indexPath, string indexType)
        {
            this.indexPath = indexPath ?? throw new ArgumentNullException(nameof(indexPath));
            this.indexType = indexType ?? throw new ArgumentNullException(nameof(indexType));
        }

        public LuceneSearchIndexWriter getWriter()
        {
            var analyzer = SetupAnalyzer();
            return
                new LuceneSearchIndexWriter
                (
            new IndexWriter(
            FSDirectory.Open(
                            Path.Combine(indexPath, indexType)
                        ),
                        new IndexWriterConfig(MATCH_LUCENE_VERSION, analyzer)
                    ),
                    new DirectoryTaxonomyWriter(
                        FSDirectory.Open(
                            Path.Combine(indexPath, indexType, "taxonomy")
                        )
                    ),
                    new FacetsConfig()
                );
        }
    }
}
