namespace golf1052.atproto.net.Models.AtProto.Repo
{
    public record GetRecordResponse<T>
    {
        public required string Uri { get; init; }
        public required T Value { get; init; }
        public string? Cid { get; set; }
    }
}
