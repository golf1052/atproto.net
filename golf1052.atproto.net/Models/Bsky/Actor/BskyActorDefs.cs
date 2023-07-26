using System.Collections.Generic;
using golf1052.atproto.net.Models.AtProto.Label;

namespace golf1052.atproto.net.Models.Bsky.Actor
{
    public record BskyProfileViewBasic
    {
        public required string Did { get; init; }
        public required string Handle { get; init; }
        public string? DisplayName { get; set; }
        public string? Avatar { get; set; }
        public BskyViewerState? Viewer { get; set; }
        public List<AtProtoLabel>? Labels { get; set; }
    }

    public record BskyViewerState
    {
        public bool? Muted { get; set; }
        public bool? BlockedBy { get; set; }
        public string? Blocking { get; set; }
        public string? Following { get; set; }
        public string? FollowedBy { get; set; }
    }
}
