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

            client.BaseAddress = new Uri(DiscordAPI.Base.API);
            client.DefaultRequestHeaders.Add("Authorization", $"Bot {Token}");
            client.DefaultRequestHeaders.Add("User-Agent", "DigiBot/0.0.0.0");

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
                    var guildMgr = new GuildManager(gatewayMgr, Logger);

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
