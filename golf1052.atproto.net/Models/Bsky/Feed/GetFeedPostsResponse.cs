using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace golf1052.atproto.net.Models.Bsky.Feed
{
    public record GetFeedPostsResponse
    {
        public required List<BskyPostView<BskyPost>> Posts { get; init; } = new List<BskyPostView<BskyPost>>();
    }
}
