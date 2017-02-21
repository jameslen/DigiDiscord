using DigiDiscord.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Source Docs: https://github.com/hammerandchisel/discord-api-docs/blob/master/docs/resources/Guild.md
//              https://github.com/hammerandchisel/discord-api-docs/blob/master/docs/resources/Channel.md

namespace DigiDiscord
{
    namespace DiscordAPI
    {
        public static class Guild
        {
            // POST, json body: 
            //{
            //    "name": string,
            //    "region": string,
            //    "icon": string,
            //    "verification_level": int,
            //    "default_message_notifications": int,
            //    "roles" : [ role objects ],
            //    "channels" : [ guild channel objects ]
            //}
            public static readonly string Create = "api/guilds";

            // GET
            public static readonly string Get = "api/guilds/{0}";

            // PATCH, json body: 
            //{
            //    "name": string,
            //    "region": string,
            //    "icon": string,
            //    "verification_level": int,
            //    "default_message_notifications": int,
            //    "afk_channel_id": string,
            //    "afk_timeout": string,
            //    "owner_id": string,
            //    "splash": string,
            //}
            public static readonly string Modify = "api/guilds/{0}";

            // DELETE
            public static readonly string Delete = "api/guilds/{0}";

            public static class Channel
            {
                // GET
                public static readonly string Get = "api/guilds/{0}/channels";

                // POST, json body
                //{
                //    "name": string,
                //    "type": string,
                //    "bitrate": int,
                //    "user_limit": int,
                //    "permission_overwrites": [ overwrites ]
                //}
                public static readonly string Create = "api/guilds/{0}/channels";

                // PATCH, json body:
                //{
                //    "id": string,
                //    "position": int
                //}
                public static readonly string ChangePosition = "api/guilds/{0}/channels";


            }

            public static class Member
            {
                // TODO: Finish
            }

            public static class Role
            {
                // TODO: Finish
            }

            public static class Ban
            {
                // TODO: Finish
            }

            public static class Prune
            {
                // TODO: Finish
            }

            public static class Integrations
            {
                // TODO: Finish
            }

            public static class Embed
            {
                // TODO: Finish
            }

            public static class Invites
            {
                // TODO: Finish
            }

            public static class Voice
            {
                // TODO: Finish
            }
        }
    }

    public class GuildMember
    {
        public DiscordUser User { get; set; }
        public List<string> Roles { get; set; }
        public bool Mute { get; set; }
        public DateTime JoinedAt { get; set; }
        public bool Deaf { get; set; }
        public string Nick { get; set; }
    }

    public class GuildChannel
    {
        public class Overwrite
        {
            public string Id { get; set; }
            public string Type { get; set; }
            public int Allow { get; set; }
            public int Deny { get; set; }
        }

        public string Id { get; set; }
        public string Guild_Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public int Position { get; set; }
        public bool Is_Private { get; set; }
        [JsonConverter(typeof(JsonListToDictionaryById<Overwrite>))]
        public Dictionary<string, Overwrite> Permission_Overwrites { get; set; }
        public string Topic { get; set; }
        public string Last_Message_Id { get; set; }
        public int? Bitrate { get; set; }
        public int? User_Limit { get; set; }
    }

    public class Guild
    {
        public class Emoji
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public List<string> Roles { get; set; }
            public bool Require_Colons { get; set; }
            public bool Managed { get; set; }
        }

        public class Role
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public int Permissions { get; set; }
            public bool Mentionable { get; set; }
            public bool Hoist { get; set; }
            public bool Managed { get; set; }
            public int Color { get; set; }
            public int Position { get; set; }
        }

        public class Presence
        {
            public DiscordUser User { get; set; }
            public string Status { get; set; }
            public Game Game { get; set; }
        }

        public class Game
        {
            public string Url { get; set; }
            public int Type { get; set; }
            public string Name { get; set; }
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public string Splash { get; set; }
        public string Owner_Id { get; set; }
        public string Region { get; set; }
        public bool EmbedEnabled { get; set; }
        public int Multifactor_Auth_Level { get; set; }
        public DateTime Joined_At { get; set; }
        public bool Large { get; set; }
        public bool? Unavailable { get; set; }
        public int Member_Count { get; set; }
        public int AFK_Timeout { get; set; }
        public string AFK_Channel_Id { get; set; }

        public List<string> Features { get; set; }
        [JsonConverter(typeof(JsonListToDictionaryByUserId<Presence>))]
        public Dictionary<string, Presence> Presences { get; set; }
        [JsonConverter(typeof(JsonListToDictionaryById<Role>))]
        public Dictionary<string,Role> Roles { get; set; }
        [JsonConverter(typeof(JsonListToDictionaryById<Emoji>))]
        public Dictionary<string,Emoji> Emojis { get; set; }
        [JsonConverter(typeof(JsonListToDictionaryByUserId<GuildMember>))]
        public Dictionary<string,GuildMember> Members { get; set; }
        [JsonConverter(typeof(JsonListToDictionaryById<GuildChannel>))]
        public Dictionary<string,GuildChannel> Channels { get; set; }
    }
}
