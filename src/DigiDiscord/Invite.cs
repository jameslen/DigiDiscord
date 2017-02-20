using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// Source Docs: https://github.com/hammerandchisel/discord-api-docs/blob/master/docs/resources/Invite.md

namespace DigiDiscord
{
    namespace DiscordAPI
    {
        public static class Invites
        {
            public static readonly string Get = "api/invites";
            // param: Invite Code
            public static readonly string Delete = "api/invites/{0}";
            // POST, param: Invite Code
            public static readonly string Accept = "api/invites/{0}";
        }
    }

        public class Invite
    {
        public class Metadata
        {
            public DiscordUser Inviter { get; set; }
            public int Uses { get; set; }
            public int MaxUses { get; set; }
            public int MaxAge { get; set; }
            public bool Temporary { get; set; }
            public DateTime CreatedAt { get; set; }
            public bool Revoked { get; set; }
        }

        public class Guild
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Splash { get; set; }
            public string Icon { get; set; }
        }

        public class Channel
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Type { get; set; }
        }

        public string Code { get; set; }
        public Guild DestinationGuild { get; set; }
        public Channel DestinationChannel { get; set; }
    }
}
