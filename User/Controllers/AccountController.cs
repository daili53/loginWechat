using System.Threading.Tasks;
using System.Web.Mvc;
using Users.Models;
using Microsoft.Owin.Security;
using System.Security.Claims;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Users.Infrastructure;
using System.Web;
using Users.Infrastructure.Wechat;
using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNet.SignalR;
using Users.Hubs;
using System;

namespace Users.Controllers
{

    [System.Web.Mvc.Authorize]
    public class AccountController : Controller
    {
        private IClient _client;

        public AccountController(IClient client)
        {
            _client = client;
        }

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                return View("Error", new string[] { "Access Denied" });
            }
            ViewBag.returnUrl = returnUrl;

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginModel details, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                AppUser user = await UserManager.FindAsync(details.Name,
                    details.Password);
                if (user == null)
                {
                    ModelState.AddModelError("", "Invalid name or password.");
                }
                else
                {
                    ClaimsIdentity ident = await UserManager.CreateIdentityAsync(user,
                        DefaultAuthenticationTypes.ApplicationCookie);
                    AuthManager.SignOut();
                    AuthManager.SignIn(new AuthenticationProperties
                    {
                        IsPersistent = false
                    }, ident);
                    return Redirect(returnUrl);
                }
            }
            ViewBag.returnUrl = returnUrl;
            return View(details);
        }

        [AllowAnonymous]
        public string WechatLogin(string state)
        {
            return _client.GetLoginLinkUri(state);
        }
        [AllowAnonymous]
        public void WechatLoginCallback(string code, string state)
        {
            var hubContex = GlobalHost.ConnectionManager.GetHubContext<ChatHub>();

            hubContex.Clients.Client(state).wechatLoginPhase2(code, state);
        }

        [AllowAnonymous]
        public async Task<ActionResult> WechatLogin2Phase(string code, string state)
        {
            UserInfo userInfo = await _client.GetUserInfo(code);

            if(userInfo != null)
            {
                UserLoginInfo loginInfo = new UserLoginInfo("WeChat", userInfo.OpenId);
                AppUser user = await UserManager.FindAsync(loginInfo);
                if(user == null)
                {
                    user = new AppUser
                    {
                        Email = string.Format("{0}@wechat", userInfo.NickName),
                        UserName = userInfo.OpenId
                    };
                    IdentityResult result = await UserManager.CreateAsync(user);
                    if (!result.Succeeded)
                        return View("Error", result.Errors);
                    else
                    {
                        result = await UserManager.AddLoginAsync(user.Id, loginInfo);
                        if(!result.Succeeded)
                        {
                            return View("Error", result.Errors);
                        }
                    }
                }

                ClaimsIdentity ident = await UserManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
                List<Claim> claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.Name, userInfo.NickName, ClaimValueTypes.String, "Wechat"));
                claims.Add(new Claim(ClaimTypes.NameIdentifier, userInfo.OpenId, ClaimValueTypes.String, "Wechat"));
                ident.AddClaims(claims);
                AuthManager.SignIn(new AuthenticationProperties { IsPersistent = false }, ident);

                return View((object)userInfo);

            }
            return View("Error", "Invalid user");
        }

        [System.Web.Mvc.Authorize]
        public ActionResult Logout()
        {
            AuthManager.SignOut();
            return RedirectToAction("Index", "Home");
        }

        private IAuthenticationManager AuthManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private AppUserManager UserManager
        {
            get
            {
                return HttpContext.GetOwinContext().GetUserManager<AppUserManager>();
            }
        }
    }
}
