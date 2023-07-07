using System.IO;

namespace golf1052.atproto.net.Models.AtProto.Repo
{
    public record UploadBlobRequest
    {
        public required Stream Content { get; init; }
        public required string MimeType { get; init; }
    }
}
