using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DigiDiscord
{
    namespace DiscordAPI
    {
        public static class Message
        {
            public static readonly string Post = "api/channels/{0}/messages";
        }
    }

    // TODO: Attachments
    // TODO: Embeds
    // TODO: Reactions
    public class Message
    {
        public class Emoji
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }
        public class Reaction
        {
            public int Count { get; set; }
            public bool Me { get; set; }
            public Emoji Emoji { get; set; }
        }

        public string Id { get; set; }
        public string Channel_Id { get; set; }
        public DiscordUser Author { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
        public DateTime Edit_Timestamp { get; set; }
        [JsonProperty(PropertyName = "tts")]
        public bool Text_To_Speech { get; set; }
        public bool Mention_Everyone { get; set; }
        public List<DiscordUser> Mentions { get; set; }
        public List<string> Mention_Roles { get; set; }
        public List<object> Attachments { get; set; }
        public List<object> Embeds { get; set; }
        public List<Reaction> Reactions { get; set; }
        public string Nonce { get; set; }
        public bool Pinned { get; set; }
        public string Webhook_Id { get; set; }
    }
}
