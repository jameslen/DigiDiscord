using DigiDiscord.Utilities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigiDiscord
{
    public enum Events
    {
        READY,
        CHANNEL_CREATE,
        CHANNEL_UPDATE,
        CHANNEL_DELETE,
        GUILD_CREATE,
        GUILD_UPDATE,
        GUILD_DELETE,
        GUILD_BAN_ADD,
        GUILD_BAN_REMOVE,
        GUILD_EMOJIS_UPDATE,
        GUILD_INTEGRATIONS_UPDATE,
        GUILD_MEMBER_ADD,
        GUILD_MEMBER_REMOVE,
        GUILD_MEMBER_UPDATE,
        GUILD_MEMBERS_CHUNK,
        GUILD_ROLE_CREATE,
        GUILD_ROLE_DELETE,
        MESSAGE_CREATE,
        MESSAGE_UPDATE,
        MESSAGE_DELETE,
        MESSAGE_DELETE_BULK,
        PRESENCE_UPDATE,
        TYPING_START,
        USER_SETTINGS_UPDATE,
        VOICE_STATE_UPDATE,
        VOICE_SERVER_UPDATE
    }

    public class GuildManager
    {
        private Dictionary<string, Guild> Guilds = new Dictionary<string, Guild>();
        private GatewayManager _Gateway;
        private ILogger _Logger;

        public GuildManager(GatewayManager gateway, ILogger logger)
        {
            _Gateway = gateway;
            _Logger = logger;

            _Gateway.EventDispatched += GatewayMessageHandler;
        }

        public delegate void GuildUpdate(Guild g);
        public event GuildUpdate GuildCreated;
        public event GuildUpdate GuildDeleted;

        public delegate void GuildChannelEventHandler(Guild guild, GuildChannel channel);
        public event GuildChannelEventHandler ChannelCreated;
        public event GuildChannelEventHandler ChannelUpdate;
        public event GuildChannelEventHandler ChannelDelete;

        public delegate void GuildBanUpdate(Guild guild, DiscordUser user);
        public event GuildBanUpdate BanAdd;
        public event GuildBanUpdate BanRemove;

        public delegate void GuildEmojiUpdate(Guild guild, List<Guild.Emoji> emojis);
        public event GuildEmojiUpdate EmojiUpdate;

        public delegate void GuildMemberUpdate(Guild guild, GuildMember member);
        public event GuildMemberUpdate MemberAdd;
        public event GuildMemberUpdate MemberRemove;
        public event GuildMemberUpdate MemberUpdate;

        public delegate void GuildMemberUpdateChunk(Guild guild, List<GuildMember> member);
        public event GuildMemberUpdateChunk MemberChunkUpdate;

        public delegate void GuildRoleUpdate(Guild guild, Guild.Role role);
        public event GuildRoleUpdate RoleCreate;
        public event GuildRoleUpdate RoleUpdate;
        public event GuildRoleUpdate RoleDelete;

        public delegate void GuildChannelMessageUpdate(GuildChannel channel, Message message);
        public event GuildChannelMessageUpdate MessageCreate;
        public event GuildChannelMessageUpdate MessageUpdate;
        public event GuildChannelMessageUpdate MessageDelete;

        public delegate void GuildChannelMessageDeleteBulk(List<Message> messages);
        public event GuildChannelMessageDeleteBulk MessagesBulkDelete;

        //TODO: Presence
        //TODO: Typing Start
        //TODO: Integrations
        //TODO: User Updates
        //TODO: Voice Chat

        private void GatewayMessageHandler(string eventName, string payload)
        {
            _Logger?.Log(LogLevel.Debug, $"{eventName} - {payload}");

            var eventValue = (Events)Enum.Parse(typeof(Events), eventName);

            var eventPayload = JObject.Parse(payload);

            switch (eventValue)
            {
                case Events.READY:
                    {
                        var guilds = eventPayload["guilds"] as JArray;

                        foreach (var guild in guilds)
                        {
                            var g = new Guild { Id = guild["id"].ToString(), Unavailable = guild["unavailable"].ToObject<bool>() };

                            Guilds.Add(g.Id, g);
                        }

                        break;
                    }
                case Events.GUILD_CREATE:
                    {
                        Guild g = null;
                        if (Guilds.ContainsKey(eventPayload["id"].ToString()))
                        {
                            //g = Guilds[eventPayload["id"].ToString()];
                        }
                        else
                        {
                            g = new Guild() { Id = eventPayload["id"].ToString() };
                            Guilds.Add(g.Id, g);
                        }

                        g = eventPayload.ToObject<Guild>();

                        Guilds[g.Id] = g;

                        break;
                    }
                case Events.CHANNEL_CREATE:
                    {
                        var c = eventPayload.ToObject<GuildChannel>();

                        break;
                    }
            }
        }
    }
}
