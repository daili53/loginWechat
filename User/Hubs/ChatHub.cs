using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace Users.Hubs
{
    public class ChatHub : Hub
    {
        public void Login()
        {
            Clients.Caller.callWechatLogin(Context.ConnectionId);
        }

    }
}