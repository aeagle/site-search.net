using Lucene.Net.Facet;
using Lucene.Net.Facet.Taxonomy.Directory;
using Lucene.Net.Index;
using System;

namespace SiteSearch.Lucene
{
    public class LuceneSearchIndexWriter : IDisposable
    {
        public IndexWriter DocsWriter { get; private set; }
        public DirectoryTaxonomyWriter TaxonomyWriter { get; private set; }
        public FacetsConfig FacetsConfig { get; private set; }

        public LuceneSearchIndexWriter(
            IndexWriter DocsWriter,
            DirectoryTaxonomyWriter TaxonomyWriter,
            FacetsConfig FacetsConfig)
        {
            this.DocsWriter = DocsWriter;
            this.TaxonomyWriter = TaxonomyWriter;
            this.FacetsConfig = FacetsConfig;
        }

        public void Dispose()
        {
            DocsWriter.Dispose();
            TaxonomyWriter.Dispose();
        }
    }
}
