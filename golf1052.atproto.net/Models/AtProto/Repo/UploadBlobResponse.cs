namespace golf1052.atproto.net.Models.AtProto.Repo
{
    public record UploadBlobResponse
    {
        public required AtProtoBlob Blob { get; init; }
    }
}
