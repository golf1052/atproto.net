using System;
using System.Collections.Generic;
using golf1052.atproto.net.Models.AtProto.Repo;
using golf1052.atproto.net.Models.Bsky.Richtext;
using Newtonsoft.Json;

namespace golf1052.atproto.net.Models.Bsky.Feed
{
    public record BskyPost
    {
        [JsonProperty("$type")]
        public const string Type = "app.bsky.feed.post";
        public required string Text { get; init; }
        public required DateTime CreatedAt { get; init; }
        public List<BskyFacet>? Facets { get; set; }
        public BskyPostReplyRef? Reply { get; set; }
    }

    public record BskyPost<T> : BskyPost
    {
        public T? Embed { get; set; }
    }

    public record BskyPostReplyRef
    {
        public required AtProtoStrongRef Root { get; init; }
        public required AtProtoStrongRef Parent { get; init; }
    }
}
