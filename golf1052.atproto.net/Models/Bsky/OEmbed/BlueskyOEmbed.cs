using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace golf1052.atproto.net.Models.Bsky.OEmbed
{
    public class BlueskyOEmbed
    {
        public string Type { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string AuthorUrl { get; set; } = string.Empty;
        public string ProviderName { get; set; } = string.Empty;
        public string ProviderUrl { get; set; } = string.Empty;
        public int CacheAge { get; set; }
        public string Html { get; set; } = string.Empty;
        public int Width { get; set; }
        public int? Height { get; set; }
    }
}
