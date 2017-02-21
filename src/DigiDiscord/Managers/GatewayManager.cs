using DigiDiscord.Utilities;
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

// Source Docs: https://github.com/hammerandchisel/discord-api-docs/blob/master/docs/topics/Gateway.md
//              

// TODO: Turn data compression on
// TODO: Voice Chat Integration
// TODO: Request Guild Members
// TODO: Set Status
// TODO: Code Cleanup
namespace DigiDiscord
{
    namespace DiscordAPI
    {
        public static class Gateway
        {
            public static readonly string Default = "api/gateway";
            public static readonly string Bot = "api/gateway/bot";
        }
    }

    public class GatewayManager
    {
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
                if (json.TryGetValue("s", out value) && value.Type != JTokenType.Null)
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

        private ClientWebSocket m_gatewaySocket = new ClientWebSocket();
        private CancellationToken m_cancellationToken = new CancellationToken();
        private int m_heartbeatInterval = 0;
        private string m_token = null;
        private string m_gateway;
        private bool m_alive = true;
        private int? m_lastRecievedSeq = null;
        private string m_sessionId = "";
        private ILogger _Logger = null;

        public delegate void EventDispatchedHandler(string eventName, string payload);
        public event EventDispatchedHandler EventDispatched;

        public GatewayManager(string gateway, string token, ILogger logger = null)
        {
            m_token = token;
            m_gateway = gateway;

            _Logger = logger;
        }

        public void Initialize()
        {
            Task.Run(async () =>
            {
                await SocketLoopHandler();

                while (m_alive) { }
            });
        }

        public void RequestGuildAllMembers(string guildId, int limit)
        {
            var guildRequestPayload = $"{{ 'guild_id': '{guildId}', 'query':'', 'limit': {limit}}}";

            SendData(string.Format(GatewayOp.GatewayPayloadBase, (int)GatewayOpCode.RequestGuildMembers, guildRequestPayload, "null", "null"));
        }

        public void RequestGuildMemberSearch(string guildId, string query, int limit)
        {
            var guildRequestPayload = $"{{ 'guild_id': '{guildId}', 'query':'{query}', 'limit': {limit}}}";

            SendData(string.Format(GatewayOp.GatewayPayloadBase, (int)GatewayOpCode.RequestGuildMembers, guildRequestPayload, "null", "null"));
        }

        protected void DispatchEvent(string eventName, string payload)
        {
            EventDispatched?.Invoke(eventName, payload);
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
                    _Logger?.Debug($"Waiting to receive data...");
                    var recv = await m_gatewaySocket.ReceiveAsync(buffer, m_cancellationToken);
                    _Logger?.Debug($"Data received.");

                    if (recv.MessageType == WebSocketMessageType.Text)
                    {
                        string payload = System.Text.Encoding.UTF8.GetString(buffer.Array, 0, recv.Count);
                        parsedData += payload;

                        _Logger?.Debug($"Websocket Data Recieved: {parsedData}");
                    }
                    else if (recv.MessageType == WebSocketMessageType.Binary)
                    {
                        // TODO: Handle the binary data
                        _Logger?.Debug("Websocket Binary Data Received");
                        _Logger?.Debug($"Recieved {recv.Count} bytes");

                        if (recv.EndOfMessage)
                        {
                            _Logger?.Debug("End of message");
                        }


                    }
                    else if (recv.MessageType == WebSocketMessageType.Close)
                    {
                        _Logger?.Info($"Connection closed. Reason: {m_gatewaySocket.CloseStatus} - {m_gatewaySocket.CloseStatusDescription}");
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
                    parsedData = "";
                    _Logger?.Error($"Exception: {ex.Message}");

                    if (!AttemptConnection(m_gateway))
                    {
                        break;
                    }
                }
            }

            _Logger?.Info($"Connection closed. Reason: {m_gatewaySocket.CloseStatus} - {m_gatewaySocket.CloseStatusDescription}");
        }

        private bool AttemptConnection(string gateway)
        {
            m_gatewaySocket = new ClientWebSocket();
            m_gatewaySocket.Options.SetRequestHeader("Authorization", $"Bot {m_token}");
            m_gatewaySocket.Options.SetRequestHeader("User-Agent", "DigiBot/0.0.0.0");

            for(int i = 0; i < 3; ++i)
            {
                _Logger?.Debug($"Connecting to gateway \"{gateway}\"");
                m_gatewaySocket.ConnectAsync(new Uri(gateway + "?v=5&encoding=json"), m_cancellationToken).Wait();

                if (m_gatewaySocket.State == WebSocketState.Open)
                {
                    _Logger?.Debug($"Connected");
                    return true;
                }

                _Logger?.Warning($"Connection attempt failed.  Trying again...");
            }

            _Logger?.Error($"Connection attempted exceeded retry limit.");
            return false;
        }

        private void ProcessMessage(GatewayOp op)
        {
            if (op.Sequence != null)
            {
                m_lastRecievedSeq = op.Sequence;
            }

            _Logger?.Debug($"GatewayOp: {op.Op}");
            switch (op.Op)
            {
                case GatewayOpCode.Dispatch:
                    if(op.EventName == "READY")
                    {
                        m_sessionId = JObject.Parse(op.Data)["session_id"].ToString();
                    }

                    DispatchEvent(op.EventName, op.Data);
                    break;
                case GatewayOpCode.Reconnect:
                    AttemptConnection(m_gateway);
                    break;       
                case GatewayOpCode.Hello:
                    m_heartbeatInterval = JObject.Parse(op.Data)["heartbeat_interval"].ToObject<int>();

                    if(string.IsNullOrEmpty(m_sessionId))
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
            _Logger?.Debug($"Sending payload: {payload}");
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

        private static string CreateHeartbeat(int? sequence)
        {
            return string.Format(GatewayOp.GatewayPayloadBase, 1, sequence == null ? "null" : sequence.Value.ToString(), "null", "null");
        }

        private static string CreateIdentity(string token, int currentShard, int totalShards)
        {
            var identity = $"{{ \"token\": \"{token}\", \"properties\": {{\"$os\": \"windows\",\"$browser\": \"digibot\",\"$device\": \"digidiscord\",\"$referrer\": \"\",\"$referring_domain\": \"\"}},\"compress\": false,\"large_threshold\": 250, \"shard\": [{currentShard}, {totalShards}] }}";
            return string.Format(GatewayOp.GatewayPayloadBase, (int)GatewayOpCode.Identify, identity, "null", "null");
        }

    }
}
