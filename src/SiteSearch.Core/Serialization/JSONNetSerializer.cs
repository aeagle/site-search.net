using Newtonsoft.Json;
using SiteSearch.Core.Interfaces;
using System.Threading.Tasks;

namespace SiteSearch.Core.Serialization
{
    public class JSONNetSerializer : ISourceSerializer
    {
        public Task<T> DeserializeAsync<T>(string text)
        {
            return Task.FromResult(JsonConvert.DeserializeObject<T>(text));
        }

        public Task<string> SerializeAsync<T>(T obj)
        {
            return Task.FromResult(JsonConvert.SerializeObject(obj));
        }
    }
}
