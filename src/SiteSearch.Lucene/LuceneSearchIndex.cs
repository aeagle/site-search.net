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
using System.Collections.Specialized;
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

        private LuceneSearchIndex() : base()
        {
            indexType = typeof(T).FullName.SafeFilename();
        }

        public LuceneSearchIndex(LuceneSearchIndexOptions options) : this()
        {
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            index = new LuceneIndex(options.IndexPath, indexType);
        }

        public async Task CreateIndexAsync(CancellationToken cancellationToken = default)
        {
            using (await index.WriterLock.WriterLockAsync(cancellationToken))
            {
                using (index.getWriter()) { };
            }
        }

        public IIngestionContext<T> StartUpdates()
        {
            return new LuceneIngestionContext<T>(index, options, searchMetaData);
        }

        public SearchQuery<T> CreateSearchQuery(NameValueCollection criteria)
        {
            return new SearchQuery<T>(this, criteria);
        }

        public async Task<SearchResult<T>> SearchAsync(SearchQuery<T> queryDefinition, CancellationToken cancellationToken = default)
        {
            using (await index.WriterLock.ReaderLockAsync(cancellationToken))
            {
                var result = new SearchResult<T>();
                result.CurrentCriteria = queryDefinition.Criteria;

                List<T> hits = new List<T>();
                var analyzer = index.SetupAnalyzer();

                using (var writer = index.getWriter())
                {
                    var mainQuery = new BooleanQuery();

                    if (queryDefinition.TermQueries.Any())
                    {
                        // Term queries
                        foreach (var termQuery in queryDefinition.TermQueries)
                        {
                            if (termQuery.field.Keyword)
                            {
                                var query = new TermQuery(
                                    new Term(termQuery.field.PropertyInfo.Name.ToLower(),
                                        escape(termQuery.value)
                                    )
                                );
                                mainQuery.Add(query, Occur.MUST);
                            }
                            else
                            {
                                var parser = new QueryParser(index.MATCH_LUCENE_VERSION, termQuery.field.PropertyInfo.Name.ToLower(), analyzer);
                                var query = parser.Parse(escape(termQuery.value));
                                mainQuery.Add(query, Occur.MUST);
                            }
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

                    // Faceting
                    if (queryDefinition.Facets.Any())
                    {
                        FacetsConfig facetsConfig = new FacetsConfig();
                        FacetsCollector fc = new FacetsCollector();
                        FacetsCollector.Search(searcher, mainQuery, queryDefinition.FacetMax, fc);

                        using (var taxonomyReader = createTaxonomyReader())
                        {
                            var facets = new FastTaxonomyFacetCounts(taxonomyReader, facetsConfig, fc);
                            foreach (var facet in queryDefinition.Facets)
                            {
                                var field = searchMetaData.Fields[facet];
                                var facetGroup = new FacetGroup(field, result.CurrentCriteria.Criteria);

                                facetGroup.Facets =
                                    facets.GetTopChildren(queryDefinition.FacetMax, facet)?
                                        .LabelValues?
                                        .Select(x => new Facet(facetGroup) { Key = x.Label, DisplayName = x.Label, Count = (long)x.Value })
                                        .Where(x => x.Count != result.TotalHits)
                                        .ToArray() ?? new Facet[0];

                                if (facetGroup.Facets.Any())
                                {
                                    result.FacetGroups.Add(facetGroup);
                                }
                            }
                        }
                    }
                }

                return result;
            }
        }

        private static readonly string[] escapableCharacters = "\\ + - && || ! ( ) { } [ ] ^ \" ~ * ? :".Split(' ');
        private static string escape(string txt)
        {
            foreach (var escapableCharacter in escapableCharacters)
            {
                txt = txt.Replace(escapableCharacter, $"\\{escapableCharacter}");
            }

            return txt;
        }

        private DirectoryTaxonomyReader createTaxonomyReader()
        {
            return new DirectoryTaxonomyReader(FSDirectory.Open(Path.Combine(options.IndexPath, indexType, "taxonomy")));
        }

        private async Task<T> inflateDocument(Document document)
        {
            return await options.Serializer.DeserializeAsync<T>(document.Get("_source"));
        }
    }
}
