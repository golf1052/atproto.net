using Newtonsoft.Json;

namespace golf1052.atproto.net.Models.AtProto.Repo
{
    public record AtProtoBlob
    {
        [JsonProperty("$type")]
        public required string Type { get; init; }
        public required AtProtoBlobRef Ref { get; init; }
        public required string MimeType { get; init; }
        public required int Size { get; init; }
    }

    public record AtProtoBlobRef
    {
        [JsonProperty("$link")]
        public required string Link { get; init; }
    }
}
