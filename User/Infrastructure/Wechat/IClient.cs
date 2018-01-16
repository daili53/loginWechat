using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Users.Infrastructure.Wechat;

namespace Users.Infrastructure.Wechat
{
    public interface IClient
    {
        string GetLoginLinkUri(string state);
        Task<UserInfo> GetUserInfo(string code);
    }
}
