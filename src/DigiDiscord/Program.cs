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

                    WebSocketLoop(url);

                    //https://discord.gg/PSSMW
                    var invite = await client.PostAsync(DiscordAPI.GetInvite + "/PSSMW",new StringContent(""));

                    var inviteData = await invite.Content.ReadAsStringAsync();

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

        public static async void WebSocketLoop(string url)
        {
            var websocket = new ClientWebSocket();

            websocket.Options.SetRequestHeader("Authorization", $"Bot {Token}");
            websocket.Options.SetRequestHeader("User-Agent", "DigiBot/0.0.0.0");

            var cancellation = new System.Threading.CancellationToken();
            var heartbeat = new System.Threading.CancellationTokenSource();
            var heartbeatToken = heartbeat.Token;

            await websocket.ConnectAsync(new Uri(url + "?v=5&encoding=json"), cancellation);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(async () =>
            {
                var bufferData = new byte[1024 * 1024 * 16];
                var buffer = new ArraySegment<byte>(bufferData);
                var socketCancel = new System.Threading.CancellationToken();
                bool connected = false;

                var idenitify = $"{{ \"op\": 2, \"d\": {{\"token\": \"{Token}\",\"properties\": {{\"$os\": \"windows\",\"$browser\": \"digibot\",\"$device\": \"digibot\",\"$referrer\": \"\",\"$referring_domain\": \"\"}},\"compress\": true,\"large_threshold\": 250, \"shard\": [0, 1]}}}}";

                while (websocket.State == WebSocketState.Open)
                {
                    try
                    {
RECV:                   var recv = await websocket.ReceiveAsync(buffer, socketCancel);

                        if (recv.MessageType == WebSocketMessageType.Text)
                        {
                            string data = System.Text.Encoding.UTF8.GetString(buffer.Array);
                            var jsonData = JObject.Parse(data);

                            Log(LogLevel.Verbose, $"Websocket Data Recieved: {jsonData}");

                            switch (jsonData["op"].ToObject<int>())
                            {
                                case 0: // Dispatch
                                    break;
                                case 1: // Heartbeat
                                    break;
                                case 3: // Status Update
                                    break;
                                case 6: // Resume
                                    break;
                                case 7: // Reconnect
                                    break;
                                case 9: // Invalid Session
                                    break;
                                case 10: // Hello
                                    if (!connected)
                                    {
                                        await websocket.SendAsync(new ArraySegment<byte>(System.Text.Encoding.UTF8.GetBytes(idenitify)), WebSocketMessageType.Text, true, cancellation);
                                        int heartbeatInterval = (jsonData["d"] as JObject)["heartbeat_interval"].ToObject<int>();
                                        Task.Run(async () =>
                                        {
                                            while (heartbeatToken.IsCancellationRequested == false)
                                            {
                                                await websocket.SendAsync(new ArraySegment<byte>(System.Text.Encoding.UTF8.GetBytes("{\"op\":11}")), WebSocketMessageType.Text, true, heartbeatToken);
                                                Thread.Sleep(heartbeatInterval);
                                            }

                                        });
                                        connected = true;
                                    }
                                    else
                                    {
                                        
                                    }

                                    break;
                                case 11: // Heartbeat ACK
                                    Log(LogLevel.Verbose, "Heartbeat ACK");
                                    break;
                            }
                        }
                        else if (recv.MessageType == WebSocketMessageType.Binary)
                        {
                            // TODO: Handle the binary data
                            Log(LogLevel.Verbose, "Websocket Binary Data Received");
                        }
                        else if (recv.MessageType == WebSocketMessageType.Close)
                        {
                            heartbeat.Cancel();
                            await websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", cancellation);
                            await websocket.ConnectAsync(new Uri(url + "?v=5&encoding=json"), cancellation);
                            heartbeat = new System.Threading.CancellationTokenSource();

                            goto RECV;
                        }
                    }
                    catch (Exception ex)
                    {
                        
                        Log(LogLevel.Error, $"Failed to receive data: {ex.Message}");

                        heartbeat.Cancel();
                        await websocket.CloseAsync(WebSocketCloseStatus.InternalServerError, "", cancellation);
                        await websocket.ConnectAsync(new Uri(url + "?v=5&encoding=json"), cancellation);
                        heartbeat = new System.Threading.CancellationTokenSource();
                    }
                }

                heartbeat.Cancel();
                Log(LogLevel.Debug, $"Connection closed: {websocket.State}");
            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        public static void Log(LogLevel level, string message)
        {
            Console.WriteLine($"[{Enum.GetName(typeof(LogLevel), level)}] {message}");
        }
    }
}
