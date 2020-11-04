using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Facet;
using Lucene.Net.Facet.Taxonomy;
using Lucene.Net.Facet.Taxonomy.Directory;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using SiteSearch.Core.Extensions;
using SiteSearch.Core.Interfaces;
using SiteSearch.Core.Models;
using SiteSearch.Core.Utils;

namespace SiteSearch.Lucene
{
    public class LuceneSearchIndex<T> : ISearchIndex<T> where T : class, new()
    {
        private const LuceneVersion MATCH_LUCENE_VERSION = LuceneVersion.LUCENE_48;
        private readonly string indexPath = @"";
        private readonly string indexType;
        private readonly SearchMetaData searchMetaData;

        private Analyzer SetupAnalyzer() => new StandardAnalyzer(MATCH_LUCENE_VERSION);

        public LuceneSearchIndex()
        {
            indexType = typeof(T).AssemblyQualifiedName.SafeFilename();
            searchMetaData = SearchMetaDataUtility.GetMetaData<T>();
        }

        public LuceneSearchIndex(LuceneSearchIndexOptions options) : this()
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            this.indexPath = options.IndexPath ?? throw new ArgumentNullException(nameof(options.IndexPath));
        }

        private LuceneSearchIndexWriter getWriter()
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

        private Document createDocument(T document)
        {
            Document doc = new Document();

            var idField = searchMetaData.Fields.FirstOrDefault(x => x.Value.Id);
            if (!idField.Equals(default(KeyValuePair<string, SearchFieldInfo>)))
            {
                doc.Add(
                    new StringField(
                        "_id",
                        idField.Value.PropertyInfo.GetValue(document).ToString(),
                        Field.Store.YES
                    )
                );
            }
            else
            {
                doc.Add(
                    new StringField(
                        "_id",
                        Guid.NewGuid().ToString(),
                        Field.Store.YES
                    )
                );
            }

            foreach (var field in searchMetaData.Fields)
            {
                var metaData = field.Value;
                var value = metaData.PropertyInfo.GetValue(document);

                if (metaData.Keyword)
                {
                    doc.Add(
                        new StringField(
                            field.Key,
                            value.ToString(),
                            metaData.Store ? Field.Store.YES : Field.Store.NO
                        )
                    );
                }
                else
                {
                    doc.Add(
                        new TextField(
                            field.Key,
                            value.ToString(),
                            metaData.Store ? Field.Store.YES : Field.Store.NO
                        )
                    );
                }
            }

            return doc;
        }

        public Task CreateIndexAsync()
        {
            using (getWriter()) { };

            return Task.CompletedTask;
        }

        public Task IndexAsync(T document)
        {
            using (var writer = getWriter())
            {
                var docToIndex = createDocument(document);
                foreach (var facetField in searchMetaData.Fields.Where(x => x.Value.Facet))
                {
                    docToIndex.Add(
                        new FacetField(
                            facetField.Key,
                            facetField.Value.PropertyInfo.GetValue(document).ToString()
                        )
                    );
                }
                var doc = writer.FacetsConfig.Build(writer.TaxonomyWriter, docToIndex);
                writer.DocsWriter.UpdateDocument(new Term("_id", docToIndex.Get("_id")), doc);

                writer.DocsWriter.Flush(triggerMerge: false, applyAllDeletes: false);
                writer.DocsWriter.Commit();
            }

            return Task.CompletedTask;
        }

        public Task IndexAsync(IEnumerable<T> documents)
        {
            using (var writer = getWriter())
            {
                foreach (var document in documents)
                {
                    var docToIndex = createDocument(document);
                    foreach (var facetField in searchMetaData.Fields.Where(x => x.Value.Facet))
                    {
                        docToIndex.Add(
                            new FacetField(
                                facetField.Key,
                                facetField.Value.PropertyInfo.GetValue(document).ToString()
                            )
                        );
                    }
                    var doc = writer.FacetsConfig.Build(writer.TaxonomyWriter, docToIndex);
                    writer.DocsWriter.UpdateDocument(new Term("_id", docToIndex.Get("_id")), docToIndex);
                }

                writer.DocsWriter.Flush(triggerMerge: false, applyAllDeletes: false);
                writer.DocsWriter.Commit();
            }

            return Task.CompletedTask;
        }

        private T inflateDocument(Document document)
        {
            var result = new T();
            foreach (var metaData in searchMetaData.Fields)
            {
                var value = document.Get(metaData.Key);

                if (value != null &&
                    metaData.Value.PropertyInfo.CanWrite &&
                    !metaData.Value.PropertyInfo.IsPropertyACollection())
                {
                    var propertyInfo = metaData.Value.PropertyInfo;

                    if (propertyInfo.PropertyType.FullName == typeof(DateTime).FullName)
                    {
                        propertyInfo.SetValue(result, DateTime.Parse(value), null);
                    }
                    else
                    {
                        var convertedValue = TypeDescriptor.GetConverter(propertyInfo.PropertyType).ConvertFromInvariantString(value);
                        propertyInfo.SetValue(result, convertedValue, null);
                    }
                }
            }

            return result;
        }

        public Task<SearchResult<T>> SearchAsync(SearchQuery queryDefinition)
        {
            var result = new SearchResult<T>();
            List<T> hits = new List<T>();

            using (var writer = getWriter())
            {
                Query query = new MatchAllDocsQuery();

                // Term queries
                if (queryDefinition.TermQueries.Any())
                {
                    var phraseQuery = new MultiPhraseQuery();
                    foreach (var termQuery in queryDefinition.TermQueries)
                    {
                        phraseQuery.Add(
                            termQuery.value
                                .Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                                .Select(phrase => new Term(termQuery.field, phrase.ToLower()))
                                .ToArray()
                        );
                    }
                    query = phraseQuery;
                }

                var reader = writer.DocsWriter.GetReader(applyAllDeletes: true);
                var searcher = new IndexSearcher(reader);
                var luceneResult = searcher.Search(query, queryDefinition.Limit);

                foreach (var doc in luceneResult.ScoreDocs)
                {
                    var foundDoc = searcher.Doc(doc.Doc);
                    hits.Add(inflateDocument(foundDoc));
                }

                result.TotalHits = luceneResult.TotalHits;
                result.Hits = hits;

                // Facets
                if (queryDefinition.Facets.Any())
                {
                    FacetsConfig facetsConfig = new FacetsConfig();
                    FacetsCollector fc = new FacetsCollector();
                    FacetsCollector.Search(searcher, query, queryDefinition.FacetMax, fc);
                    using (var taxonomyReader = new DirectoryTaxonomyReader(FSDirectory.Open(Path.Combine(indexPath, indexType, "taxonomy"))))
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

            return Task.FromResult(result);
        }
    }
}
