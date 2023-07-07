namespace golf1052.atproto.net.Models.AtProto.Repo
{
    public record GetRecordRequest
    {
        public required string Repo { get; init; }
        public required string Collection { get; init; }
        public required string Rkey { get; init; }
        public string? Cid { get; init; }
    }
}
