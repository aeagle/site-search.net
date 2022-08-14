using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SiteSearch.Core.Interfaces
{
    public interface IIngestionContext<T> : IDisposable
    {
        Task IndexAsync(T document, CancellationToken cancellationToken = default);
        Task DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task IndexAsync(IEnumerable<T> documents, CancellationToken cancellationToken = default);
    }
}
