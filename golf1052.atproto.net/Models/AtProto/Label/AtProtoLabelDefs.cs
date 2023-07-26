namespace golf1052.atproto.net.Models.AtProto.Label
{
    public record AtProtoLabel
    {
        public required string Src { get; init; }
        public required string Uri { get; init; }
        public string? Cid { get; set; }
        public required string Val { get; init; }
        public bool? Neg { get; set; }
        public required string Cts { get; init; }
    }
}
