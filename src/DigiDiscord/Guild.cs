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

        public static class GuildChannel
        {
            public static readonly string Get = "api/channels/{0}";

            // PUT, PATCH
            public static readonly string Update = "api/channels/{0}";

            public static readonly string Delete = "api/channels/{0}";

            // GET, query string params: 
            // around, before, after, limit
            public static readonly string GetMessages = "api/channels/{0}/messages";

            // GET
            public static readonly string GetMessage = "api/channels/{0}/messages/{1}";

            // POST, json body:
            // {
            //     content: string,
            //     nonce: string,
            //     tts: bool,
            //     file: file content,
            //     embed: embed object
            // }
            public static readonly string CreateMessage = "api/channels/{0}/messages";
        }
    }

    public enum Permissions
    {
        CREATE_INSTANT_INVITE   = 0x00000001,
        KICK_MEMBERS            = 0x00000002,
        BAN_MEMBERS             = 0x00000004,
        ADMINISTRATOR           = 0x00000008,
        MANAGE_CHANNELS         = 0x00000010,
        MANAGE_GUILD            = 0x00000020,
        ADD_REACTIONS           = 0x00000040,
        READ_MESSAGES           = 0x00000400,
        SEND_MESSAGES           = 0x00000800,
        SEND_TTS_MESSAGES       = 0x00001000,
        MANAGE_MESSAGES         = 0x00002000,
        EMBED_LINKS             = 0x00004000,
        ATTACH_FILES            = 0x00008000,
        READ_MESSAGE_HISTORY    = 0x00010000,
        MENTION_EVERYONE        = 0x00020000,
        USE_EXTERNAL_EMOJIS     = 0x00040000,
        CONNECT                 = 0x00100000,
        SPEAK                   = 0x00200000,
        MUTE_MEMBERS            = 0x00400000,
        DEAFEN_MEMBERS          = 0x00800000,
        MOVE_MEMBERS            = 0x01000000,
        USE_VAD                 = 0x02000000,
        CHANGE_NICKNAME         = 0x04000000,
        MANAGE_NICKNAMES        = 0x08000000,
        MANAGE_ROLES            = 0x10000000,
        MANAGE_WEBHOOKS         = 0x20000000,
        MANAGE_EMOJIS           = 0x40000000
    }

    public class GuildMember
    {
        public DiscordUser User { get; set; }
        public List<string> Roles { get; set; }
        public bool Mute { get; set; }
        public DateTime JoinedAt { get; set; }
        public bool Deaf { get; set; }
        public string Nick { get; set; }
        public int Permissions { get; set; }
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
        public Guild Guild { get; set; }
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

        // TODO: Add events to channels

        public async Task SendMessage(string message, bool tts = false)
        {
            var messagePayload = $"{{\"content\": \"{message}\", \"tts\": {tts.ToString().ToLower()}}}";

            await Discord.Instance.Post<Message>(string.Format(DiscordAPI.GuildChannel.CreateMessage, Id), messagePayload);
        }
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

        // TODO: Add events to Guilds
        // User member join/leave/update/chunk, guild update, channel create/update/delete, ban add/delete, emoji, role c/u/d

        public void UpdateAllUserPermissions()
        {
            foreach(var member in Members.Values)
            {
                UpdateUserPermission(member);
            }
        }

        public void UpdateUserPermission(GuildMember member)
        {
            member.Permissions = 0;
            foreach(var role in member.Roles)
            {
                member.Permissions |= Roles[role].Permissions;
            }
        }
    }
}
