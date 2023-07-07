using System;
using Newtonsoft.Json;

namespace golf1052.atproto.net.Models.Bsky.Feed
{
    public record BskyPost
    {
        [JsonProperty("$type")]
        public const string Type = "app.bsky.feed.post";
        public required string Text { get; init; }
        public required DateTime CreatedAt { get; init; }
    }

    public record Post<T> : BskyPost
    {
        public T? Embed { get; set; }
    }
}
