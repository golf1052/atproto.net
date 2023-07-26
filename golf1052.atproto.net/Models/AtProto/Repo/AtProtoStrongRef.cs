namespace golf1052.atproto.net.Models.AtProto.Repo
{
    public record AtProtoStrongRef
    {
        public required string Uri { get; init; }
        public required string Cid { get; init; }
    }
}
