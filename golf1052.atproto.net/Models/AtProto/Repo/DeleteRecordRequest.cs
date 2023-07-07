namespace golf1052.atproto.net.Models.AtProto.Repo
{
    public record DeleteRecordRequest
    {
        public required string Repo { get; init; }
        public required string Collection { get; init; }
        public required string Rkey { get; init; }
        public string? SwapRecord { get; set; }
        public string? SwapCommit { get; set; }
    }
}
