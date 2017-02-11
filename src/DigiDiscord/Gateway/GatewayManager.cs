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
        private int m_heartbeatInterval = 0;
        private string m_token = null;
        private bool m_alive = true;
        private int? m_lastRecievedSeq = null;

        //public delegate void GatewayOpReceived(Gateway)

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
            public static string GatewayPayloadBase = "{{ \"op\": {0}, \"d\": {1}, \"s\": {2}, \"t\": {3} }}";

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
            var identity = $"{{ \"token\": \"{token}\", \"properties\": {{\"$os\": \"windows\",\"$browser\": \"digibot\",\"$device\": \"digibot\",\"$referrer\": \"\",\"$referring_domain\": \"\"}},\"compress\": false,\"large_threshold\": 250, \"shard\": [{currentShard}, {totalShards}] }}";
            return string.Format(GatewayOp.GatewayPayloadBase, (int)GatewayOpCode.Identify, identity, "null", "null");
        }


        private async Task SocketLoopHandler(string gateway)
        {
            var bufferData = new byte[1024 * 16];
            var buffer = new ArraySegment<byte>(bufferData);

            Program.Log(LogLevel.Verbose, $"Connecting to gateway \"{gateway}\"");
            await m_gatewaySocket.ConnectAsync(new Uri(gateway + "?v=5&encoding=json"), m_cancellationToken);
            Program.Log(LogLevel.Verbose, $"Connected");

            string parsedData = "";

            while (m_gatewaySocket.State == WebSocketState.Open)
            {
                try 
                {
                    Program.Log(LogLevel.Verbose, $"Waiting to receive data...");
                    var recv = await m_gatewaySocket.ReceiveAsync(buffer, m_cancellationToken);
                    Program.Log(LogLevel.Verbose, $"Data received.");

                    if (recv.MessageType == WebSocketMessageType.Text)
                    {
                        string payload = System.Text.Encoding.UTF8.GetString(buffer.Array, 0, recv.Count);
                        parsedData += payload;

                        Program.Log(LogLevel.Verbose, $"Websocket Data Recieved: {parsedData}");
                    }
                    else if (recv.MessageType == WebSocketMessageType.Binary)
                    {
                        // TODO: Handle the binary data
                        Program.Log(LogLevel.Verbose, "Websocket Binary Data Received");
                        Program.Log(LogLevel.Verbose, $"Recieved {recv.Count} bytes");

                        if (recv.EndOfMessage)
                        {
                            Program.Log(LogLevel.Verbose, "End of message");
                        }


                    }
                    else if (recv.MessageType == WebSocketMessageType.Close)
                    {
                        //heartbeat.Cancel();
                        //await websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", cancellation);
                        //await websocket.ConnectAsync(new Uri(url + "?v=5&encoding=json"), cancellation);
                        //heartbeat = new System.Threading.CancellationTokenSource();
                    }
                    
                    if(recv.EndOfMessage)
                    {
                        var payload = new GatewayOp(parsedData);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        ProcessMessage(payload);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                        parsedData = "";
                    }

                }
                catch (Exception ex)
                {

                    Program.Log(LogLevel.Error, $"Failed to receive data: {ex.Message}");

                    break;
                }
            }

            Program.Log(LogLevel.Verbose, $"Connection closed. Reason: {m_gatewaySocket.CloseStatus} - {m_gatewaySocket.CloseStatusDescription}");
        }

        private async Task ProcessMessage(GatewayOp op)
        {
            if (op.Sequence != null)
            {
                m_lastRecievedSeq = op.Sequence;
            }

            Program.Log(LogLevel.Verbose, $"GatewayOp: {op.Op}");
            switch (op.Op)
            {
                case GatewayOpCode.Dispatch: // Dispatch
                    if (op.EventName == "MESSAGE_CREATE")
                    {
                        //EchoMessage()
                    }
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
                    {
                        var response = CreateIdentity(m_token, 0, 1);
                        await SendData(response);
                        m_heartbeatInterval = JObject.Parse(op.Data)["heartbeat_interval"].ToObject<int>();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        SendData($"{{\"op\":1, \"d\":null}}");
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    }
                    //else
                    {

                    }

                    break;
                case GatewayOpCode.HeartbeatACK: // Heartbeat ACK
                    Program.Log(LogLevel.Verbose, "Heartbeat ACK");
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    Task.Run(() =>
                    {
                        Thread.Sleep(m_heartbeatInterval);
                        SendData($"{{\"op\":1, \"d\":{m_lastRecievedSeq}}}");
                    });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    break;
            }
        }

        private async Task SendData(string payload)
        {
            Program.Log(LogLevel.Verbose, $"Sending payload: {payload}");
            await m_gatewaySocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(payload)), WebSocketMessageType.Text, true, m_cancellationToken);
        }

    }
}
