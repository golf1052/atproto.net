namespace golf1052.atproto.net.Models.AtProto.Server
{
    public record CreateSessionRequest
    {
        public required string Identifier { get; init; }
        public required string Password { get; init; }
    }
}
