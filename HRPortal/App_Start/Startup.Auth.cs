using HRPortal.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using System;

namespace HRPortal
{
    public partial class Startup
    {

        // 啟用應用程式以使用 OAuthAuthorization。如此您就可以保護 Web APIs
        //static Startup()
        //{
        //    PublicClientId = "web";

        //    OAuthOptions = new OAuthAuthorizationServerOptions
        //    {
        //        TokenEndpointPath = new PathString("/Token"),
        //        AuthorizeEndpointPath = new PathString("/Account/Authorize"),
        //        Provider = new ApplicationOAuthProvider(PublicClientId),
        //        AccessTokenExpireTimeSpan = TimeSpan.FromDays(14),
        //        AllowInsecureHttp = true
        //    };
        //}

        //public static OAuthAuthorizationServerOptions OAuthOptions { get; private set; }

        //public static string PublicClientId { get; private set; }

        public void ConfigureAuth(IAppBuilder app)
        {
            // 讓應用程式使用 Cookie 儲存已登入使用者的資訊
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/"), //2071102 Daniel 登入路徑由/Login改為根路徑
                ExpireTimeSpan = TimeSpan.FromMinutes(20),//設定閒置自動登出時間
                SlidingExpiration = true,
                Provider = new CookieAuthenticationProvider
                {
                    // 讓應用程式在使用者登入時驗證安全性戳記。
                    // 這是您變更密碼或將外部登入新增至帳戶時所使用的安全性功能。  
                    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, ApplicationUser, Guid>(
                        validateInterval: TimeSpan.FromDays(30),
                        regenerateIdentityCallback: (manager, user) =>
                        user.GenerateUserIdentityAsync(manager),
                    getUserIdCallback: user => Guid.Parse(user.GetUserId()))
                }
            });
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);
        }

        //// 如需設定驗證的詳細資訊，請瀏覽 http://go.microsoft.com/fwlink/?LinkId=301864
        //public void ConfigureAuth(IAppBuilder app)
        //{
        //    // 設定資料庫內容、使用者管理員和登入管理員，以針對每個要求使用單一執行個體
        //    app.CreatePerOwinContext(ApplicationDbContext.Create);
        //    app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
        //    app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);

        //    // 讓應用程式使用 Cookie 儲存已登入使用者的資訊
        //    app.UseCookieAuthentication(new CookieAuthenticationOptions
        //    {
        //        AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
        //        LoginPath = new PathString("/Account/Login"),
        //        Provider = new CookieAuthenticationProvider
        //        {
        //            // 讓應用程式在使用者登入時驗證安全性戳記。
        //            // 這是您變更密碼或將外部登入新增至帳戶時所使用的安全性功能。  
        //            OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, ApplicationUser>(
        //                validateInterval: TimeSpan.FromMinutes(20),
        //                regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager))
        //        }
        //    });
        //    // 使用 Cookie 暫時儲存使用者利用協力廠商登入提供者登入的相關資訊
        //    app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

        //    // 讓應用程式在雙因素驗證程序中驗證第二個因素時暫時儲存使用者資訊。
        //    app.UseTwoFactorSignInCookie(DefaultAuthenticationTypes.TwoFactorCookie, TimeSpan.FromMinutes(5));

        //    // 讓應用程式記住第二個登入驗證因素 (例如電話或電子郵件)。
        //    // 核取此選項之後，將會在用來登入的裝置上記住登入程序期間的第二個驗證步驟。
        //    // 這類似於登入時的 RememberMe 選項。
        //    app.UseTwoFactorRememberBrowserCookie(DefaultAuthenticationTypes.TwoFactorRememberBrowserCookie);

        //    // 讓應用程式使用 Bearer 權杖驗證使用者
        //    app.UseOAuthBearerTokens(OAuthOptions);

        //    // 註銷下列各行以啟用利用協力廠商登入提供者登入
        //    //app.UseMicrosoftAccountAuthentication(
        //    //    clientId: "",
        //    //    clientSecret: "");

        //    //app.UseTwitterAuthentication(
        //    //    consumerKey: "",
        //    //    consumerSecret: "");

        //    //app.UseFacebookAuthentication(
        //    //    appId: "",
        //    //    appSecret: "");

        //    //app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions()
        //    //{
        //    //    ClientId = "",
        //    //    ClientSecret = ""
        //    //});
        //}
    }
}
