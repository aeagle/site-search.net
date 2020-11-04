using Microsoft.Extensions.Primitives;
using System.IO;
using System.Linq;

namespace SiteSearch.Core.Extensions
{
    public static class StringExtensions
    {
        private static readonly char[] invalidFileNameChars = Path.GetInvalidFileNameChars();

        public static string SafeFilename(this string text) => 
            new string(text.Select(ch => invalidFileNameChars.Contains(ch) ? '_' : ch).ToArray());

        public static int? ParseInt(this string text) =>
            !string.IsNullOrWhiteSpace(text) && int.TryParse(text, out var num) ? num : (int?)null;

        public static int? ParseInt(this StringValues text) =>
            ParseInt(text.ToString());
    }
}
