using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DigiDiscord
{
    namespace DiscordAPI
    {
        public static class User
        {
            // Parameter is user.id
            public static readonly string Get = "api/users/{0}";
            // Parameter is user.id, json body { "usersname" : string, "avatar" : avatar data }
            public static readonly string Patch = "api/users/{0}";

            // GET
            public static readonly string Guilds = "api/users/{0}/guilds";
            // DELETE
            public static readonly string LeaveGuild = "api/users/{0}/guild/{1}";

            // GET
            public static readonly string DirectMessageChannels = "api/users/@me/channels";
            // POST, json body: { "recipient_id": string }
            public static readonly string NewDirectMessageChannel = "api/users/@me/channels";
            // POST, json body: { "access_tokens: [ tokens ], "nicks": dictionary of nick to user id }
            public static readonly string GroupDirectMessageChannel = "api/users/@me/channels";

        }
    }

        public class DiscordUser
    {
        public string Username { get; set; }
        public string Id { get; set; }
        public string Discriminator { get; set; }
        public string Avatar { get; set; }
        public bool Bot { get; set; }
        public bool MFAEnabled { get; set; }
        public bool Verified { get; set; }
        public string Email { get; set; }
    }
}
