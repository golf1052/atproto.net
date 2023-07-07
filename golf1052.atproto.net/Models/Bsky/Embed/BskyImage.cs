using golf1052.atproto.net.Models.AtProto.Repo;

namespace golf1052.atproto.net.Models.Bsky.Embed
{
    public record BskyImage
    {
        public required AtProtoBlob Image { get; init; }
        public required string Alt { get; init; }
    }
}
