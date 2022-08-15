using Lucene.Net.Documents;
using Lucene.Net.Facet;
using Lucene.Net.Facet.Taxonomy.Directory;
using Lucene.Net.Index;
using Lucene.Net.Store;
using SiteSearch.Core.Interfaces;
using SiteSearch.Lucene;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SiteSearch.Core.Models
{
    public class LuceneIngestionContext<T> : IIngestionContext<T>, IDisposable where T : class, new()
    {
        private readonly LuceneIndex index;
        private readonly LuceneSearchIndexOptions options;
        private readonly SearchMetaData searchMetaData;
        private readonly string indexType;
        private readonly IDisposable writerLock;
        private readonly LuceneSearchIndexWriter writer;
        
        public LuceneIngestionContext(
            LuceneIndex index,
            LuceneSearchIndexOptions options,
            SearchMetaData searchMetaData,
            string indexType)
        { 
            this.index = index ?? throw new ArgumentNullException(nameof(index));
            this.options = options ?? throw new ArgumentNullException(nameof(options));
            this.searchMetaData = searchMetaData ?? throw new ArgumentNullException(nameof(searchMetaData));
            this.indexType = indexType ?? throw new ArgumentNullException(nameof(indexType));
            writerLock = index.WriterLock.WriterLock();
            writer = getWriter();
        }

        public async Task IndexAsync(T document, CancellationToken cancellationToken = default)
        {
            var docToIndex = await createDocument(document);
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

        public async Task IndexAsync(IEnumerable<T> documents, CancellationToken cancellationToken = default)
        {
            foreach (var document in documents)
            {
                var docToIndex = await createDocument(document);
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
        }

        public Task DeleteAsync(string id, CancellationToken cancellationToken = default)
        {
            writer.DocsWriter.DeleteDocuments(new Term("_id", id));
            return Task.CompletedTask;
        }

        private bool disposed = false;
        public void Dispose()
        {
            if (!disposed)
            {
                writer.DocsWriter.Flush(triggerMerge: false, applyAllDeletes: false);
                writer.DocsWriter.Commit();
                writer.Dispose();
                writerLock.Dispose();
                disposed = true;
            }
        }

        private async Task<Document> createDocument(T document)
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

            // Store document as JSON in _source field
            doc.Add(
                new StringField(
                    "_source",
                    await options.Serializer.SerializeAsync(document),
                    Field.Store.YES
                )
            );

            foreach (var field in searchMetaData.Fields)
            {
                var metaData = field.Value;
                var value = metaData.PropertyInfo.GetValue(document);

                if (value != default)
                {
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
            }

            return doc;
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
    }
}
