using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DigiDiscord.Gateway
{
    public class GatewayManager
    {
        private ClientWebSocket m_gatewaySocket = new ClientWebSocket();
        private CancellationToken m_cancellationToken = new CancellationToken();
        private string m_token = null;
        private bool m_alive = true;

        public GatewayManager(string gateway, string token)
        {
            m_token = token;
            m_gatewaySocket.Options.SetRequestHeader("Authorization", $"Bot {token}");
            m_gatewaySocket.Options.SetRequestHeader("User-Agent", "DigiBot/0.0.0.0");

            Task.Run(async () => 
            {
                await SocketLoopHandler(gateway); 
                
                while (m_alive) { } 
            });
        }

        private enum GatewayOpCode : int
        {
            Dispatch = 0,
            Heartbeat,
            Identify,
            StatusUpdate,
            VoiceStateUpdate,
            VoiceServerPing,
            Resume,
            Reconnect,
            RequestGuildMembers,
            InvalidSession,
            Hello,
            HeartbeatACK
        }

        private class GatewayOp
        {
            public static string GatewayPayloadBase = "{{ 'op': {0}, 'd': {1}, 's': {2}, 't': {3} }}";

            public GatewayOp(string payload)
            {
                JObject json = JObject.Parse(payload);

                Op = (GatewayOpCode)json["op"].ToObject<int>();
                Data = json["d"].ToString();

                JToken value = null;
                if(json.TryGetValue("s", out value) && value.Type != JTokenType.Null)
                {
                    Sequence = value.ToObject<int>();
                }

                if (json.TryGetValue("t", out value) && value.Type != JTokenType.Null)
                {
                    EventName = value.ToString();
                }
            }

            public GatewayOpCode Op { get; set; }
            public int? Sequence { get; set; } = null;
            public string Data { get; set; } = null;
            public string EventName { get; set; } = null;
        }

        private static string CreateHeartbeat(int? sequence)
        {
            return string.Format(GatewayOp.GatewayPayloadBase, GatewayOpCode.Heartbeat, sequence == null ? "null" : sequence.Value.ToString(), "null", "null");
        }

        private static string CreateIdentity(string token, int currentShard, int totalShards)
        {
            var identity = $"{{ 'token': {token}, 'properties': {{'$os': 'windows','$browser': 'digibot','$device': 'digibot','$referrer': '','$referring_domain': ''}},'compress': true,'large_threshold': 250, 'shard': [{currentShard}, {totalShards}] }}";
            return string.Format(GatewayOp.GatewayPayloadBase, (int)GatewayOpCode.Identify, identity, "null", "null");
        }


        private async Task SocketLoopHandler(string gateway)
        {
            var bufferData = new byte[1024 * 16];
            var buffer = new ArraySegment<byte>(bufferData);

            await m_gatewaySocket.ConnectAsync(new Uri(gateway + "?v=5&encoding=json"), m_cancellationToken);

            while(m_gatewaySocket.State == WebSocketState.Open)
            {
                try 
                {
                    var recv = await m_gatewaySocket.ReceiveAsync(buffer, m_cancellationToken);

                    if (recv.MessageType == WebSocketMessageType.Text)
                    {
                        string data = System.Text.Encoding.UTF8.GetString(buffer.Array, 0, recv.Count);

                        var payload = new GatewayOp(data);

                        Program.Log(LogLevel.Verbose, $"Websocket Data Recieved: {data}");

                        switch (payload.Op)
                        {
                            case GatewayOpCode.Dispatch: // Dispatch
                                break;
                            case GatewayOpCode.Heartbeat: // Heartbeat
                                break;
                            case GatewayOpCode.StatusUpdate: // Status Update
                                break;
                            case GatewayOpCode.Resume: // Resume
                                break;
                            case GatewayOpCode.Reconnect: // Reconnect
                                break;
                            case GatewayOpCode.InvalidSession: // Invalid Session
                                break;
                            case GatewayOpCode.Hello: // Hello
                                //if (!connected)
                                {
                                    await m_gatewaySocket.SendAsync(new ArraySegment<byte>(System.Text.Encoding.UTF8.GetBytes(CreateIdentity(m_token,0,1))), WebSocketMessageType.Text, true, m_cancellationToken);
                                    //int heartbeatInterval = (jsonData["d"] as JObject)["heartbeat_interval"].ToObject<int>();
                                    //Task.Run(async () =>
                                    //{
                                    //    while (heartbeatToken.IsCancellationRequested == false)
                                    //    {
                                    //        await websocket.SendAsync(new ArraySegment<byte>(System.Text.Encoding.UTF8.GetBytes("{\"op\":11}")), WebSocketMessageType.Text, true, heartbeatToken);
                                    //        Thread.Sleep(heartbeatInterval);
                                    //    }
                                    //
                                    //});
                                    //connected = true;
                                }
                                //else
                                {

                                }

                                break;
                            case GatewayOpCode.HeartbeatACK: // Heartbeat ACK
                                Program.Log(LogLevel.Verbose, "Heartbeat ACK");
                                break;
                        }
                    }
                    else if (recv.MessageType == WebSocketMessageType.Binary)
                    {
                        // TODO: Handle the binary data
                        Program.Log(LogLevel.Verbose, "Websocket Binary Data Received");
                    }
                    else if (recv.MessageType == WebSocketMessageType.Close)
                    {
                        //heartbeat.Cancel();
                        //await websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", cancellation);
                        //await websocket.ConnectAsync(new Uri(url + "?v=5&encoding=json"), cancellation);
                        //heartbeat = new System.Threading.CancellationTokenSource();
                    }


                }
                catch (Exception ex)
                {

                    Program.Log(LogLevel.Error, $"Failed to receive data: {ex.Message}");

                    break;
                }
            }
        }

    }
}
