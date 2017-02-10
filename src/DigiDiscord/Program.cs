using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;


namespace DigiDiscord
{
    internal class DiscordAPI
    {
        public static readonly string ApiBase = "https://discordapp.com/";
        public static readonly string Gateway = "api/gateway";
        public static readonly string BotGateway = "api/gateway/bot";
        public static readonly string GetInvite = "api/invites";
    }
    
    public enum LogLevel
    {
        Debug,
        Verbose,
        Info,
        Warning,
        Error
    }

    public class Program
    {
        internal static string Token = "";

        public static void Main(string[] args)
        {
            AsyncMain(args).Wait();
            Console.ReadLine();
        }

        public static async Task AsyncMain(string[] args)
        {
            Token = File.ReadAllText("Token.txt");
            var client = new HttpClient();

            client.BaseAddress = new Uri(DiscordAPI.ApiBase);
            client.DefaultRequestHeaders.Add("Authorization", $"Bot {Token}");
            client.DefaultRequestHeaders.Add("User-Agent", "DigiBot/0.0.0.0");

            try
            {
                Log(LogLevel.Verbose, "Retrieving gateway...");
                var gateway = await client.GetAsync(DiscordAPI.BotGateway);

                if (gateway.IsSuccessStatusCode)
                {
                    var result = await gateway.Content.ReadAsStringAsync();
                    Log(LogLevel.Verbose, $"Gateway returned: {result}");

                    var json = JObject.Parse(result);

                    var url = json["url"].ToString();

                    var gatewayMgr = new Gateway.GatewayManager(url, Token);

                    //https://discord.gg/PSSMW
                    //var invite = await client.PostAsync(DiscordAPI.GetInvite + "/PSSMW",new StringContent(""));

                    //var inviteData = await invite.Content.ReadAsStringAsync();

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
            Console.WriteLine($"[{Enum.GetName(typeof(LogLevel), level)}] {message}");
        }
    }
}
