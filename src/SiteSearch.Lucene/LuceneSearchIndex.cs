using Lucene.Net.Documents;
using Lucene.Net.Facet;
using Lucene.Net.Facet.Taxonomy;
using Lucene.Net.Facet.Taxonomy.Directory;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using SiteSearch.Core;
using SiteSearch.Core.Extensions;
using SiteSearch.Core.Interfaces;
using SiteSearch.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SiteSearch.Lucene
{
    public class LuceneSearchIndex<T> : BaseSearchIndex<T>, ISearchIndex<T> where T : class, new()
    {
        private readonly string indexType;
        private readonly LuceneSearchIndexOptions options;
        private readonly LuceneIndex index;

        public LuceneSearchIndex() : base()
        {
            indexType = typeof(T).AssemblyQualifiedName.SafeFilename();
            index = new LuceneIndex();
        }

        public LuceneSearchIndex(LuceneSearchIndexOptions options) : this()
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task CreateIndexAsync(CancellationToken cancellationToken = default)
        {
            using (await index.WriterLock.WriterLockAsync(cancellationToken))
            {
                using (getWriter()) { };
            }
        }

        public IIngestionContext<T> StartUpdates()
        {
            return new LuceneIngestionContext<T>(index, options, searchMetaData, indexType);
        }

        public async Task<SearchResult<T>> SearchAsync(SearchQuery queryDefinition, CancellationToken cancellationToken = default)
        {
            using (await index.WriterLock.ReaderLockAsync(cancellationToken))
            {
                var result = new SearchResult<T>();
                List<T> hits = new List<T>();
                var analyzer = index.SetupAnalyzer();

                using (var writer = getWriter())
                {
                    var mainQuery = new BooleanQuery();

                    if (queryDefinition.TermQueries.Any())
                    {
                        // Term queries
                        foreach (var termQuery in queryDefinition.TermQueries)
                        {
                            var parser = new QueryParser(index.MATCH_LUCENE_VERSION, termQuery.field, analyzer);
                            var query = parser.Parse(termQuery.value);
                            mainQuery.Add(query, Occur.MUST);
                        }
                    }
                    else
                    {
                        mainQuery.Add(new MatchAllDocsQuery(), Occur.MUST);
                    }

                    var reader = writer.DocsWriter.GetReader(applyAllDeletes: true);
                    var searcher = new IndexSearcher(reader);
                    var luceneResult = searcher.Search(mainQuery, queryDefinition.Limit);

                    foreach (var doc in luceneResult.ScoreDocs)
                    {
                        var foundDoc = searcher.Doc(doc.Doc);
                        hits.Add(await inflateDocument(foundDoc));
                    }

                    result.TotalHits = luceneResult.TotalHits;
                    result.Hits = hits;

                    // Facets
                    if (queryDefinition.Facets.Any())
                    {
                        FacetsConfig facetsConfig = new FacetsConfig();
                        FacetsCollector fc = new FacetsCollector();
                        FacetsCollector.Search(searcher, mainQuery, queryDefinition.FacetMax, fc);
                        using (var taxonomyReader = new DirectoryTaxonomyReader(FSDirectory.Open(Path.Combine(options.IndexPath, indexType, "taxonomy"))))
                        {
                            var facets = new FastTaxonomyFacetCounts(taxonomyReader, facetsConfig, fc);
                            foreach (var facet in queryDefinition.Facets)
                            {
                                var facetGroup = new FacetGroup { Field = facet };
                                facetGroup.Facets =
                                    facets.GetTopChildren(queryDefinition.FacetMax, facet).LabelValues
                                        .Select(x => new Facet { Key = x.Label, Count = (long)x.Value })
                                        .ToArray();
                                result.FacetGroups.Add(facetGroup);
                            }
                        }
                    }
                }

                return result;
            }
        }

        private LuceneSearchIndexWriter getWriter()
        {
            var analyzer = index.SetupAnalyzer();
            return
                new LuceneSearchIndexWriter
                (
                    new IndexWriter(
                        FSDirectory.Open(
                            Path.Combine(options.IndexPath, indexType)
                        ),
                        new IndexWriterConfig(index.MATCH_LUCENE_VERSION, analyzer)
                    ),
                    new DirectoryTaxonomyWriter(
                        FSDirectory.Open(
                            Path.Combine(options.IndexPath, indexType, "taxonomy")
                        )
                    ),
                    new FacetsConfig()
                );
        }

        private async Task<T> inflateDocument(Document document)
        {
            return await options.Serializer.DeserializeAsync<T>(document.Get("_source"));
        }
    }
}
