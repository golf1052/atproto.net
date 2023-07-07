using System.Collections.Generic;
using Newtonsoft.Json;

namespace golf1052.atproto.net.Models.Bsky.Embed
{
    public record BskyImages
    {
        [JsonProperty("$type")]
        public const string Type = "app.bsky.embed.images";
        public required List<BskyImage> Images { get; init; }
    }
}
