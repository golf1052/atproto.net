using System.Collections.Generic;
using golf1052.atproto.net.Models.AtProto.Label;
using golf1052.atproto.net.Models.Bsky.Actor;

namespace golf1052.atproto.net.Models.Bsky.Feed
{
    public record BskyReplyRef<T>
    {
        public required BskyPostView<T> Root { get; init; }
        public required BskyPostView<T> Parent { get; init; }
    }

    public record BskyPostView<T>
    {
        public required string Uri { get; init; }
        public required string Cid { get; init; }
        public required BskyProfileViewBasic Author { get; init; }
        public required T Record { get; init; }
        public int? ReplyCount { get; set; }
        public int? RepostCount { get; set; }
        public int? LikeCount { get; set; }
        public required string IndexedAt { get; init; }
        public BskyViewerState? Viewer { get; set; }
        public List<AtProtoLabel>? Labels { get; set; }
    }

    public record BskyPostView<T, U> : BskyPostView<T>
    {
        public U? Embed { get; set; }
    }

    public record BskyViewerState
    {
        public string? Repost { get; set; }
        public string? Like { get; set; }
    }
}
