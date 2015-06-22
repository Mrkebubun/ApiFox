using System;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Security.OAuth;
using Owin;
using ApiFox.Models;
using ApiFox.Providers;
using System.IO;
using ApiFox.Extensions;
using System.Xml.Linq;
using System.Web;
using System.Configuration;

namespace ApiFox
{
    public partial class Startup
    {
        // Enable the application to use OAuthAuthorization. You can then secure your Web APIs
        static Startup()
        {
            PublicClientId = "web";

            OAuthOptions = new OAuthAuthorizationServerOptions
            {
                TokenEndpointPath = new PathString("/token"),
                AuthorizeEndpointPath = new PathString("/Account/Authorize"),
                Provider = new ApplicationOAuthProvider(PublicClientId),
                AccessTokenExpireTimeSpan = TimeSpan.FromDays(14),
                AllowInsecureHttp = true
            };
        }

        public static OAuthAuthorizationServerOptions OAuthOptions { get; private set; }

        public static string PublicClientId { get; private set; }

        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            // Configure the db context and user manager to use a single instance per request
            app.CreatePerOwinContext(ApplicationDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);

            // Enable the application to use a cookie to store information for the signed in user
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
                Provider = new CookieAuthenticationProvider
                {
                    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, ApplicationUser>(
                        validateInterval: TimeSpan.FromMinutes(20),
                        regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager))
                }
            });
            // Use a cookie to temporarily store information about a user logging in with a third party login provider
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // Enable the application to use bearer tokens to authenticate users
             
            app.UseOAuthBearerTokens(OAuthOptions);

            var appSettings = ConfigurationManager.AppSettings;
            
          
            // enable logging in with third party login providers
            //if (appSettings["MicrosoftAccountAuthenticationClientId"] != null && appSettings["MicrosoftAccountAuthenticationClientSecret"] != null)
            //    app.UseMicrosoftAccountAuthentication(
            //        clientId: appSettings["MicrosoftAccountAuthenticationClientId"],
            //        clientSecret: appSettings["MicrosoftAccountAuthenticationClientSecret"]);

            if (appSettings["TwitterAuthenticationConsumerKey"] != null && appSettings["TwitterAuthenticationConsumerSecret"] != null)
            app.UseTwitterAuthentication(
                consumerKey: appSettings["TwitterAuthenticationConsumerKey"],
                consumerSecret: appSettings["TwitterAuthenticationConsumerSecret"]);

            if (appSettings["FacebookAuthenticationAppId"] != null && appSettings["FacebookAuthenticationAppSecret"] != null)
            app.UseFacebookAuthentication(
                appId: appSettings["FacebookAuthenticationAppId"],
                appSecret: appSettings["FacebookAuthenticationAppSecret"]);

            //todo use oauth2
            //app.UseGoogleAuthentication();
        }
    }
}
