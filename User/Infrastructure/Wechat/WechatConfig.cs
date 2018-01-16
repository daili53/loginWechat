using System.Configuration;

namespace Users.Infrastructure.Wechat
{
    public class WechatConfigSection: ConfigurationSection
    {
        [ConfigurationProperty("clientId", IsRequired =true)]
        public string ClientId
        {
            get { return (string)this["clientId"]; }
            set { this["clientId"] = value; }
        }

        [ConfigurationProperty("clientSecret", IsRequired = true)]
        public string ClientSecret
        {
            get { return (string)this["clientSecret"]; }
            set { this["clientSecret"] = value; }
        }

        [ConfigurationProperty("scope", IsRequired = true)]
        public string Scope
        {
            get { return (string)this["scope"]; }
            set { this["scope"] = value; }
        }
        [ConfigurationProperty("redirectUrl", IsRequired =true)]
        public string RedirectUrl
        {
            get { return (string)this["redirectUrl"]; }
            set { this["redirectUrl"] = value; }
        }
    }
}