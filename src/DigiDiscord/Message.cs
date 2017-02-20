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
        public string ChannelId { get; set; }
        public DiscordUser Author { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
        public DateTime EditTimestamp { get; set; }
        public bool TextToSpeech { get; set; }
        public bool MentionEveryone { get; set; }
        public List<DiscordUser> Mentions { get; set; }
        public List<string> RoleMentions { get; set; }
        public List<object> Attachments { get; set; }
        public List<object> Embeds { get; set; }
        public List<Reaction> Reactions { get; set; }
        public string Nonce { get; set; }
        public bool Pinned { get; set; }
        public string WebhookId { get; set; }
    }
}
