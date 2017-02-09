using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigiDiscord.Gateway
{
    public enum GatewayOpCode
    {
        Dispatch,
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

    public class GatewayOp
    {
        private readonly string GatewayPayloadBase = "{ 'op': {0}, 'd': {1}, 's': {2}, 't': {3} }";
    }
}
