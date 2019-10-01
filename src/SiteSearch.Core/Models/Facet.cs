namespace SiteSearch.Core.Models
{
    public class Facet
    {
        public string Key { get; set; }
        public long Count { get; set; }

        public override string ToString()
        {
            return $"{Key} ({Count})";
        }
    }
}
