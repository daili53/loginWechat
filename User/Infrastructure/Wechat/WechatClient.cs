using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Web.Configuration;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Users.Infrastructure.Wechat
{
    public class UserInfo
    {
        [JsonProperty("openid")]
        public string OpenId;

        [JsonProperty("nickname")]
        public string NickName;

        [JsonProperty("headimgurl")]
        public string HeadimgUrl;
    }

    public class Token
    {
        [JsonProperty("access_token")]
        public string AccessToken;

        [JsonProperty("expires_in")]
        public int expiresIn;

        [JsonProperty("refresh_token")]
        public string RefreshToken;

        [JsonProperty("openid")]
        public string openId;
        public string scope;
    }

    public class WechatClient : IClient
    {
        private string _appId;
        private string _appSecret;
        private string _scope;
        private string _redirectUri;
        public const string ResponseType = "code";
        public const string AccessCodeEndpoint = "https://open.weixin.qq.com/connect/oauth2/authorize";
        private HttpClient _client; 

        public WechatClient()
        {
            WechatConfigSection settings = WebConfigurationManager.GetWebApplicationSection("wechatConfig") as WechatConfigSection;
            if(settings != null)
            {
                _appId = settings.ClientId;
                _appSecret = settings.ClientSecret;
                _scope = settings.Scope;
                _redirectUri = HttpUtility.UrlEncode(settings.RedirectUrl);
            }
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public string GetLoginLinkUri(string state)
        {
           return string.Format("{0}?appid={1}&redirect_uri={2}&response_type={3}&scope={4}&state={5}#wechat_redirect",
                AccessCodeEndpoint, _appId, _redirectUri, ResponseType, _scope, state);
        }

        private async Task<Token> GetToken(string code)
        {
            string path = string.Format("https://api.weixin.qq.com/sns/oauth2/access_token?appid={0}&secret={1}&code={2}&grant_type=authorization_code", _appId, _appSecret, code);

            Token token = null;
            HttpResponseMessage response = await _client.GetAsync(path);
            if(response.IsSuccessStatusCode)
            {
                var tokenString = await response.Content.ReadAsStringAsync();
                token = JsonConvert.DeserializeObject<Token>(tokenString);
            }
            return token;
        }
        public async Task<UserInfo> GetUserInfo(string code)
        {
            Token token = await GetToken(code);
            string path = string.Format("https://api.weixin.qq.com/sns/userinfo?access_token={0}&openid={1}&lang=zh_CN", token.AccessToken, token.openId);
            UserInfo userInfo = null;
            HttpResponseMessage response = await _client.GetAsync(path);
            if(response.IsSuccessStatusCode)
            {
                var userInfos = await response.Content.ReadAsStringAsync();
                userInfo = JsonConvert.DeserializeObject<UserInfo>(userInfos);
            }
            return userInfo;
        }
    }

}