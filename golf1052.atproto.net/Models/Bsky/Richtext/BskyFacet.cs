using System.Collections.Generic;
using Newtonsoft.Json;

namespace golf1052.atproto.net.Models.Bsky.Richtext
{
    public record BskyFacet
    {
        public required BskyByteSlice Index { get; init; }
        public required List<BskyFeature> Features { get; init; }
    }

    public record BskyFeature
    {
    }

    public record BskyMention : BskyFeature
    {
        [JsonProperty("$type")]
        public const string Type = "app.bsky.richtext.facet#mention";
        public required string Did { get; init; }
    }

    public record BskyLink : BskyFeature
    {
        [JsonProperty("$type")]
        public const string Type = "app.bsky.richtext.facet#link";
        public required string Uri { get; init; }
    }

    public record BskyByteSlice
    {
        public required int ByteStart { get; init; }
        public required int ByteEnd { get; init; }
    }
}
