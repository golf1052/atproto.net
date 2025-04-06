using Newtonsoft.Json;

namespace golf1052.atproto.net.Models.Bsky.Embed
{
    public record BskyRecord
    {
        [JsonProperty("$type")]
        public const string Type = "app.bsky.embed.record";
        public required BskyViewRecord Record { get; init; }
    }

    public record BskyViewRecord
    {
        public required string Uri { get; init; }
        public required string Cid { get; init; }
    }
}
