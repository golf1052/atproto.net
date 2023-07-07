using System.Collections.Generic;

namespace golf1052.atproto.net.Models.AtProto.Repo
{
    public record DescribeRepoResponse<T>
    {
        public required string Handle { get; init; }
        public required string Did { get; init; }
        public required T DidDoc { get; init; }
        public required List<string> Collections { get; init; }
        public required bool HandleIsCorrect { get; init; }
    }
}
