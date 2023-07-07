namespace golf1052.atproto.net.Models.AtProto.Server
{
    public record GetSessionResponse
    {
        public required string Did { get; init; }
        public required string Handle { get; init; }
        public string? Email { get; set; }
    }
}
