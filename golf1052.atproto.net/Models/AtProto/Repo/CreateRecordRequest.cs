namespace golf1052.atproto.net.Models.AtProto.Repo
{
    public record CreateRecordRequest<T>
    {
        public required string Repo { get; init; }
        public required string Collection { get; init; }
        public required T Record { get; init; }
        public string? Rkey { get; set; }
        public bool? Validate { get; set; }
        public string? SwapCommit { get; set; }
    }
}
