using DigiDiscord.Utilities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace DigiDiscord
{
    public class Discord
    {
        public GatewayManager Gateway { get; private set; }
        public GuildManager GuildManager { get; private set; }

        private string m_token;
        private HttpClient m_httpClient = new HttpClient();
        private ILogger _Logger;

        public Discord(string token)
        {
            m_token = token;

            m_httpClient.BaseAddress = new Uri(DiscordAPI.Base.API);
            m_httpClient.DefaultRequestHeaders.Add("Authorization", $"Bot {token}");
            m_httpClient.DefaultRequestHeaders.Add("User-Agent", $"DigiBot/{typeof(Discord).GetTypeInfo().Assembly.ImageRuntimeVersion}");
        }

        // TODO: User sign in
        public Discord(string username, string password)
        {
            throw new NotImplementedException("User sign-in not supported at this time.");
        }

        public async Task Initialize(ILogger logger)
        {
            _Logger = logger;

            _Logger?.Verbose("Retrieving gateway...");
            var gateway = await m_httpClient.GetAsync(DiscordAPI.Gateway.Bot);

            if (gateway.IsSuccessStatusCode)
            {
                var result = await gateway.Content.ReadAsStringAsync();
                _Logger?.Verbose($"Gateway returned: {result}");

                var json = JObject.Parse(result);

                var url = json["url"].ToString();

                Gateway = new GatewayManager(url, m_token, _Logger);
                GuildManager = new GuildManager(Gateway, _Logger);

                Gateway.Initialize();
            }
            else
            {
                _Logger?.Error($"Error returning gateway: {gateway.ReasonPhrase}");
            }
        }
    }
}
