using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

// TODO: Voice Chat Integration
// TODO: Request Guild Members
// TODO: Set Status
// TODO: Code Cleanup
namespace DigiDiscord.Gateway
{
    public class GatewayManager
    {
        private ClientWebSocket m_gatewaySocket = new ClientWebSocket();
        private CancellationToken m_cancellationToken = new CancellationToken();
        private int m_heartbeatInterval = 0;
        private string m_token = null;
        private string m_gateway;
        private bool m_alive = true;
        private int? m_lastRecievedSeq = null;
        private int m_sessionId = -1;

        public delegate void EventDispatchedHandler(string eventName, string payload);

        public event EventDispatchedHandler EventDispatched;

        protected void DispatchEvent(string eventName, string payload)
        {
            EventDispatched?.Invoke(eventName, payload);
        }

        public GatewayManager(string gateway, string token)
        {
            m_token = token;
            m_gateway = gateway;

            Task.Run(async () => 
            {
                await SocketLoopHandler(); 
                
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
            return string.Format(GatewayOp.GatewayPayloadBase, 1, sequence == null ? "null" : sequence.Value.ToString(), "null", "null");
        }

        private static string CreateIdentity(string token, int currentShard, int totalShards)
        {
            var identity = $"{{ \"token\": \"{token}\", \"properties\": {{\"$os\": \"windows\",\"$browser\": \"digibot\",\"$device\": \"digibot\",\"$referrer\": \"\",\"$referring_domain\": \"\"}},\"compress\": false,\"large_threshold\": 250, \"shard\": [{currentShard}, {totalShards}] }}";
            return string.Format(GatewayOp.GatewayPayloadBase, (int)GatewayOpCode.Identify, identity, "null", "null");
        }

        private async Task SocketLoopHandler()
        {
            var bufferData = new byte[1024 * 16];
            var buffer = new ArraySegment<byte>(bufferData);

            if(AttemptConnection(m_gateway) == false)
            {
                return;
            }

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
                        Program.Log(LogLevel.Verbose, $"Connection closed. Reason: {m_gatewaySocket.CloseStatus} - {m_gatewaySocket.CloseStatusDescription}");
                        if (!AttemptConnection(m_gateway))
                        {
                            break;
                        }
                    }
                    
                    if(recv.EndOfMessage)
                    {
                        var payload = new GatewayOp(parsedData);
                        ProcessMessage(payload);
                        parsedData = "";
                    }

                }
                catch (Exception ex)
                {

                    Program.Log(LogLevel.Error, $"Exception: {ex.Message}");

                    if (!AttemptConnection(m_gateway))
                    {
                        break;
                    }
                }
            }

            Program.Log(LogLevel.Verbose, $"Connection closed. Reason: {m_gatewaySocket.CloseStatus} - {m_gatewaySocket.CloseStatusDescription}");
        }

        private bool AttemptConnection(string gateway)
        {
            m_gatewaySocket = new ClientWebSocket();
            m_gatewaySocket.Options.SetRequestHeader("Authorization", $"Bot {m_token}");
            m_gatewaySocket.Options.SetRequestHeader("User-Agent", "DigiBot/0.0.0.0");

            for(int i = 0; i < 3; ++i)
            {
                Program.Log(LogLevel.Verbose, $"Connecting to gateway \"{gateway}\"");
                m_gatewaySocket.ConnectAsync(new Uri(gateway + "?v=5&encoding=json"), m_cancellationToken).Wait();

                if (m_gatewaySocket.State == WebSocketState.Open)
                {
                    Program.Log(LogLevel.Verbose, $"Connected");
                    return true;
                }

                Program.Log(LogLevel.Verbose, $"Connection attempt failed.  Trying again...");
            }

            Program.Log(LogLevel.Error, $"Connection attempted exceeded retry limit.");
            return false;
        }

        private void ProcessMessage(GatewayOp op)
        {
            if (op.Sequence != null)
            {
                m_lastRecievedSeq = op.Sequence;
            }

            Program.Log(LogLevel.Verbose, $"GatewayOp: {op.Op}");
            switch (op.Op)
            {
                case GatewayOpCode.Dispatch: // Dispatch
                    if(op.EventName == "READY")
                    {
                        m_sessionId = JObject.Parse(op.Data)["session_id"].ToObject<int>();
                    }

                    DispatchEvent(op.EventName, op.Data);
                    break;
                case GatewayOpCode.Reconnect:
                    AttemptConnection(m_gateway);
                    break;       
                case GatewayOpCode.Hello:
                    m_heartbeatInterval = JObject.Parse(op.Data)["heartbeat_interval"].ToObject<int>();

                    if(m_sessionId == -1)
                    {
                        SendIdentity();
                    }
                    else
                    {
                        var payload = new JObject();
                        payload["token"] = m_token;
                        payload["session_id"] = m_sessionId;
                        payload["seq"] = m_lastRecievedSeq;

                        SendData(payload.ToString());
                    }
                    break;
                case GatewayOpCode.InvalidSession:
                    SendIdentity();
                    break;
                case GatewayOpCode.HeartbeatACK:
                    SendHeartbreat();
                    break;
            }
        }

        private void SendIdentity()
        {
            var response = CreateIdentity(m_token, 0, 1);
            SendData(response);
            SendHeartbreat();
        }

        private void SendData(string payload)
        {
            Program.Log(LogLevel.Verbose, $"Sending payload: {payload}");
            m_gatewaySocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(payload)), WebSocketMessageType.Text, true, m_cancellationToken);
        }

        private void SendHeartbreat()
        {
            Task.Run(() =>
            {
                Thread.Sleep(m_heartbeatInterval);
                SendData(CreateHeartbeat(m_lastRecievedSeq));
            });
        }

    }
}
