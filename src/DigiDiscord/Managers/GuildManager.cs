﻿using DigiDiscord.Utilities;
using Newtonsoft.Json;
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
        GUILD_ROLE_UPDATE,
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
        public event GuildUpdate GuildUpdated;
        public event GuildUpdate GuildDeleted;

        public delegate void GuildChannelEventHandler(Guild guild, GuildChannel channel);
        public event GuildChannelEventHandler ChannelCreate;
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

        public delegate void GuildMemberUpdateChunk(Guild guild, List<GuildMember> members);
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
            _Logger?.Debug($"{eventName} - {payload}");

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
                            g = Guilds[eventPayload["id"].ToString()];
                        }
                        else
                        {
                            g = new Guild() { Id = eventPayload["id"].ToString() };
                            Guilds.Add(g.Id, g);
                        }

                        JsonConvert.PopulateObject(payload, g);

                        Guilds[g.Id] = g;

                        GuildCreated?.Invoke(g);
                        break;
                    }
                case Events.GUILD_UPDATE:
                    {
                        var g = Guilds[eventPayload["id"].ToString()];

                        // TODO: Double check that this is the correct behavior.
                        JsonConvert.PopulateObject(payload, g);

                        GuildUpdated?.Invoke(g);
                        break;
                    }
                case Events.GUILD_DELETE:
                    {
                        var g = Guilds[eventPayload["id"].ToString()];
                        Guilds.Remove(eventPayload["id"].ToString());

                        GuildDeleted?.Invoke(g);
                        break;
                    }
                case Events.CHANNEL_CREATE:
                    {
                        var c = eventPayload.ToObject<GuildChannel>();
                        var g = Guilds[c.Guild_Id];

                        g.Channels.Add(c.Id, c);

                        ChannelCreate?.Invoke(g, c);
                        break;
                    }
                case Events.CHANNEL_UPDATE:
                    {
                        var g = Guilds[eventPayload["guild_id"].ToString()];
                        var c = g.Channels[eventPayload["id"].ToString()];

                        JsonConvert.PopulateObject(payload, c);

                        ChannelUpdate?.Invoke(g, c);
                        break;
                    }
                case Events.CHANNEL_DELETE:
                    {
                        var g = Guilds[eventPayload["guild_id"].ToString()];
                        var c = g.Channels[eventPayload["id"].ToString()];

                        g.Channels.Remove(c.Id);

                        ChannelDelete?.Invoke(g, c);
                        break;
                    }
                case Events.GUILD_BAN_ADD:
                    {
                        var g = Guilds[eventPayload["guild_id"].ToString()];
                        var m = g.Members[eventPayload["id"].ToString()];

                        g.Members.Remove(m.User.Id);

                        BanAdd?.Invoke(g, m.User);
                        break;
                    }
                case Events.GUILD_BAN_REMOVE:
                    {
                        var g = Guilds[eventPayload["guild_id"].ToString()];

                        BanRemove?.Invoke(g, eventPayload.ToObject<DiscordUser>());
                        break;
                    }
                case Events.GUILD_EMOJIS_UPDATE:
                    {
                        var g = Guilds[eventPayload["guild_id"].ToString()];

                        g.Emojis.Clear();

                        JsonConvert.PopulateObject(payload, g);

                        EmojiUpdate?.Invoke(g, eventPayload["emojis"].ToObject<List<Guild.Emoji>>());
                        break;
                    }
                case Events.GUILD_MEMBER_ADD:
                    {
                        var g = Guilds[eventPayload["guild_id"].ToString()];
                        var m = eventPayload.ToObject<GuildMember>();

                        g.Members.Add(m.User.Id, m);
                        g.Member_Count++;

                        MemberAdd?.Invoke(g, m);
                        break;
                    }
                case Events.GUILD_MEMBER_REMOVE:
                    {
                        var g = Guilds[eventPayload["guild_id"].ToString()];
                        var m = g.Members[eventPayload["user"]["id"].ToString()];

                        g.Members.Remove(m.User.Id);

                        MemberRemove?.Invoke(g, m);
                        break;
                    }
                case Events.GUILD_MEMBER_UPDATE:
                    {
                        var g = Guilds[eventPayload["guild_id"].ToString()];
                        var m = g.Members[eventPayload["user"]["id"].ToString()];

                        JsonConvert.PopulateObject(payload, m);

                        MemberUpdate?.Invoke(g, m);
                        break;
                    }
                case Events.GUILD_MEMBERS_CHUNK:
                    {
                        var g = Guilds[eventPayload["guild_id"].ToString()];
                        var members = eventPayload["members"].ToObject<List<GuildMember>>();

                        foreach(var member in members)
                        {
                            g.Members.Remove(member.User.Id);
                            g.Members.Add(member.User.Id, member);
                        }
                        
                        MemberChunkUpdate?.Invoke(g, members);
                        break;
                    }
                case Events.GUILD_ROLE_CREATE:
                    {
                        var g = Guilds[eventPayload["guild_id"].ToString()];
                        var r = eventPayload["role"].ToObject<Guild.Role>();

                        g.Roles.Add(r.Id, r);

                        RoleCreate?.Invoke(g, r);
                        break;
                    }
                case Events.GUILD_ROLE_UPDATE:
                    {
                        var g = Guilds[eventPayload["guild_id"].ToString()];
                        var r = g.Roles[eventPayload["role"]["id"].ToString()];

                        JsonConvert.PopulateObject(eventPayload["role"].ToString(), r);

                        RoleUpdate?.Invoke(g, r);
                        break;
                    }
                case Events.GUILD_ROLE_DELETE:
                    {
                        var g = Guilds[eventPayload["guild_id"].ToString()];
                        var r = g.Roles[eventPayload["role_id"].ToString()];

                        g.Roles.Remove(r.Id);

                        RoleDelete?.Invoke(g, r);
                        break;
                    }
                case Events.MESSAGE_CREATE:
                    {
                        break;
                    }
                case Events.MESSAGE_UPDATE:
                    {
                        break;
                    }
                case Events.MESSAGE_DELETE:
                    {
                        break;
                    }
                case Events.MESSAGE_DELETE_BULK:
                    {
                        break;
                    }
            }
        }
    }
}
