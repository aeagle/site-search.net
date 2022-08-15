using System.Collections.Specialized;
using System.Linq;
using System.Net;

namespace SiteSearch.Core.Extensions
{
    public static class NameValueCollectionExtensions
    {
        public static NameValueCollection AddCriteria(this NameValueCollection existing, string key, string value)
        {
            var newCriteria = new NameValueCollection(existing);
            newCriteria.Add(key, value);
            return newCriteria;
        }

        public static NameValueCollection RemoveCriteria(this NameValueCollection existing, string key, string value)
        {
            var newCriteria = new NameValueCollection(existing);
            newCriteria.Remove(key);
            return newCriteria;
        }

        public static string AsQueryString(this NameValueCollection collection)
        {
            return string.Join("&", collection.AllKeys.Select(a => a + "=" + WebUtility.UrlEncode(collection[a])));
        }
    }
}
