using System.Threading.Tasks;

namespace SiteSearch.Core.Interfaces
{
    public interface ISourceSerializer
    {
        Task<T> DeserializeAsync<T>(string text);
        Task<string> SerializeAsync<T>(T obj);
    }
}
