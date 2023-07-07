namespace golf1052.atproto.net.Models.AtProto.Server
{
    public record CreateSessionResponse
    {
        public required string Did { get; init; }
        public required string Handle { get; init; }
        public required string AccessJwt { get; init; }
        public required string RefreshJwt { get; init; }
        public string? Email { get; set; }
    }
}
