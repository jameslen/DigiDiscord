using DigiDiscord.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static DigiDiscord.Guild;

namespace DigiDiscord
{
    namespace DiscordAPI
    {
        public class Base
        {
            public static readonly string API = "https://discordapp.com/";
        }
    }

    namespace Internal
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

        public class Member
        {
            public DiscordUser User { get; set; }
            public List<string> Roles { get; set; }
            public bool Mute { get; set; }
            public string Joined_At { get; set; }
            public bool Deaf { get; set; }
            public string Nick { get; set; }
        }

        public class PermissionOverwrite
        {
            public string Role { get; set; }
            public string Id { get; set; }
            public int Deny { get; set; }
            public int Allow { get; set; }
        }

        public class Channel
        {
            public string Type { get; set; }
            public string Topic { get; set; }
            public int Position { get; set; }
            public List<PermissionOverwrite> Permission_Overwrites { get; set; }
            public string Name { get; set; }
            public string Last_Pin_Timestamp { get; set; }
            public string Last_Message_Id { get; set; }
            public string Id { get; set; }
            public bool Is_Private { get; set; }
            public DiscordUser Recipient { get; set; }
        }

        public class Guild
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Icon { get; set; }
            public string Splash { get; set; }
            public string Owner_Id { get; set; }
            public string Region { get; set; }
            public bool EmbebEnabled { get; set; }
            public List<DigiDiscord.Guild.Role> Roles { get; set; }
            public List<DigiDiscord.Guild.Emoji> Emojis { get; set; }
            public List<string> Features { get; set; }
            public int MultifactorAuthLevel { get; set; }
            public DateTime Joined_At { get; set; }
            public bool Large { get; set; }
            public bool Unavailable { get; set; }
            public int MemberCount { get; set; }
            public List<Member> Members { get; set; }
            public List<Channel> Channels { get; set; }
            public List<DigiDiscord.Guild.Presence> Presences { get; set; }
            public int AFK_Timeout { get; set; }
            public string AFK_Channel_Id { get; set; }
        }
    }

    public class GuildManager
    {
        private Dictionary<string,Guild> Guilds = new Dictionary<string, Guild>();
    }

    public class ConsoleLogger : ILogger
    {
        private LogLevel m_minLevel;

        public ConsoleLogger(LogLevel minLevel = LogLevel.Verbose)
        {
            m_minLevel = minLevel;
        }

        public override void Log(LogLevel level, string logLine)
        {
            if(level >= m_minLevel)
            {
                PrintLevel(level);
                PrintLine(logLine);
            }
        }

        private void PrintLevel(LogLevel level)
        {
            var bg = Console.BackgroundColor;
            var fg = Console.ForegroundColor;

            switch(level)
            {
                case LogLevel.Error:
                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogLevel.Warning:
                    Console.BackgroundColor = ConsoleColor.Yellow;
                    Console.ForegroundColor = ConsoleColor.Black;
                    break;
                case LogLevel.Info:
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    break;
                case LogLevel.Verbose:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    break;
                case LogLevel.Debug:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
            }

            Console.Write($"[{level}] ");

            Console.BackgroundColor = bg;
            Console.ForegroundColor = fg;
        }

        private void PrintLine(string line)
        {
            Console.WriteLine(line);
        }
    }

    public class Program
    {
        internal static string Token = "";

        public static void Main(string[] args)
        {
            AsyncMain(args).Wait();
            Console.ReadLine();
        }

        public static HttpClient client;
        public static ILogger Logger = new ConsoleLogger(LogLevel.Debug);

        public static async Task AsyncMain(string[] args)
        {
            Token = File.ReadAllText("Token.txt");
            client = new HttpClient();

            client.BaseAddress = new Uri(DiscordAPI.ApiBase);
            client.DefaultRequestHeaders.Add("Authorization", $"Bot {Token}");
            client.DefaultRequestHeaders.Add("User-Agent", "DigiBot/0.0.0.0");

            Dictionary<string, Internal.Guild> Guilds = new Dictionary<string, Internal.Guild>();

            try
            {
                Log(LogLevel.Verbose, "Retrieving gateway...");
                var gateway = await client.GetAsync(DiscordAPI.Gateway.Bot);

                if (gateway.IsSuccessStatusCode)
                {
                    var result = await gateway.Content.ReadAsStringAsync();
                    Log(LogLevel.Verbose, $"Gateway returned: {result}");

                    var json = JObject.Parse(result);

                    var url = json["url"].ToString();

                    var gatewayMgr = new GatewayManager(url, Token, new ConsoleLogger());

                    gatewayMgr.EventDispatched += (eventName, payload) =>
                    {
                        Log(LogLevel.Debug, $"{eventName} - {payload}");

                        var eventValue = (Internal.Events)Enum.Parse(typeof(Internal.Events), eventName);

                        var eventPayload = JObject.Parse(payload);

                        switch(eventValue)
                        {
                            case Internal.Events.READY:
                            {
                                var guilds = eventPayload["guilds"] as JArray;

                                foreach(var guild in guilds)
                                {
                                    var g = new Internal.Guild { Id = guild["id"].ToString(), Unavailable = guild["unavailable"].ToObject<bool>() };

                                    Guilds.Add(g.Id, g);
                                }

                                break;
                            }
                            case Internal.Events.GUILD_CREATE:
                            {
                                    Internal.Guild g = null;
                                if(Guilds.ContainsKey(eventPayload["id"].ToString()))
                                {
                                    g = Guilds[eventPayload["id"].ToString()];
                                }
                                else
                                {
                                    g = new Internal.Guild() { Id = eventPayload["id"].ToString() };
                                    Guilds.Add(g.Id, g);
                                }

                                g = eventPayload.ToObject<Internal.Guild>();

                                Guilds[g.Id] = g;

                                break;
                            }
                            case Internal.Events.CHANNEL_CREATE:
                            {
                                    Internal.Channel c = eventPayload.ToObject<Internal.Channel>();

                                break;
                            }
                        }
                    };

                    gatewayMgr.Initialize();

                    while (true)
                    {
                        
                    }
                }
                else
                {
                    Log(LogLevel.Error, $"Error returning gateway: {gateway.ReasonPhrase}");
                }
            }
            catch (Exception e)
            {
                Log(LogLevel.Error, $"{e.Message}");
            }
        }

        public static void Log(LogLevel level, string message)
        {
            Logger.Log(level, message);
        }
    }
}
