using DigiDiscord.Utilities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DigiDiscord
{
    namespace DiscordAPI
    {
        public class Base
        {
            public static readonly string API = "https://discordapp.com/";
        }
    }

    public class Discord
    {
        public GatewayManager Gateway { get; private set; }
        public GuildManager GuildManager { get; private set; }

        private static Discord _instance = null;

        public static Discord Instance 
        { 
            get
            {
                if(_instance == null)
                {
                    _instance = new Discord();
                }

                return _instance;
            }
        }

        private string m_token;
        // TODO: This needs to be refactored or shared so the other classes can make calls
        internal HttpClient m_httpClient = new HttpClient();
        private ILogger _Logger;

        private Discord()
        {
            m_httpClient.BaseAddress = new Uri(DiscordAPI.Base.API);
            m_httpClient.DefaultRequestHeaders.Add("User-Agent", $"DigiBot/{typeof(Discord).GetTypeInfo().Assembly.ImageRuntimeVersion}");
        }

        public async Task InitializeBot(string token, ILogger logger = null)
        {
            m_token = token;
            
            m_httpClient.DefaultRequestHeaders.Add("Authorization", $"Bot {token}");

            await Initialize(logger);
        }

        // TODO: User sign in
        private async Task InitializeUser(string username, string password, ILogger logger = null)
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

        //internal async Task<T> Get<T>(string api)
        //{
        //    //m_httpClient.GetAsync()
        //}

        internal async Task<T> Post<T>(string api, string payload) where T : class
        {
            var result = await m_httpClient.PostAsync(api, new StringContent(payload, Encoding.UTF8, "application/json"));

            if(result.IsSuccessStatusCode)
            {
                return JObject.Parse(await result.Content.ReadAsStringAsync()).ToObject<T>();
            }

            return null;
        }
    }
}
