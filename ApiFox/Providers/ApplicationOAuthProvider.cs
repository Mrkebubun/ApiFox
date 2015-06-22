using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
 
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OAuth;
using System.Web;
using Microsoft.AspNet.Identity.Owin;

namespace ApiFox.Providers
{
    public class ApplicationOAuthProvider : OAuthAuthorizationServerProvider
    {
        private readonly string _publicClientId;

        public ApplicationOAuthProvider(string publicClientId)
        {
            if (publicClientId == null)
            {
                throw new ArgumentNullException("publicClientId");
            }

            _publicClientId = publicClientId;
        }

        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            if (context.ClientId == null)
            {
                context.Validated();
            }

            return Task.FromResult<object>(null);
        }

        public override Task ValidateTokenRequest(OAuthValidateTokenRequestContext context)
        {
            context.Validated();
            return base.ValidateTokenRequest(context);
        }

        public override Task ValidateClientRedirectUri(OAuthValidateClientRedirectUriContext context)
        {
            if (context.ClientId == _publicClientId)
            {
                Uri expectedRootUri = new Uri(context.Request.Uri, "/");

                if (expectedRootUri.AbsoluteUri == context.RedirectUri)
                {
                    context.Validated();
                }
                else if (context.ClientId == "web")
                {
                    var expectedUri = new Uri(context.Request.Uri, "/");
                    context.Validated(expectedUri.AbsoluteUri);
                }
            }

            return Task.FromResult<object>(null);
        }

        /// <summary>
        /// Add parameters to the response
        /// </summary>
        /// <param name="context">Endpoint context</param>
        /// <returns>Task</returns>		
        public override Task TokenEndpoint(OAuthTokenEndpointContext context)
        {
            foreach (KeyValuePair<string, string> property in context.Properties.Dictionary)
            {
                context.AdditionalResponseParameters.Add(property.Key, property.Value);
            }

            return Task.FromResult<object>(null);
        }


        /// <summary>
        /// oAuth Resource Password Login Flow
        /// 1. Checks the password with the Identity API
        /// 2. Create a user identity for the bearer token
        /// 3. Create a user identity for the cookie
        /// 4. Calls the context.Validated(ticket) to tell the oAuth2 server to protect the ticket as an access token and send it out in JSON payload
        /// 5. Signs the cookie identity so it can send the authentication cookie
        /// </summary>
        /// <param name="context">The authorization context</param>
        /// <returns>Task</returns>		
        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            using (var userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>())
            {
                userManager.MaxFailedAccessAttemptsBeforeLockout = 50;
                userManager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);

                var user = await userManager.FindByNameAsync(context.UserName);

                if (user == null)
                {
                    context.SetError("invalid_grant", "Invalid username");
                    return;
                }

                if (await userManager.IsLockedOutAsync(user.Id))
                {
                    var timeleft = user.LockoutEndDateUtc.GetValueOrDefault().Subtract(DateTime.UtcNow);

                    var timetype = timeleft.Minutes == 0 ? "seconds" : "minute(s)";
                    var timevalue = timeleft.Minutes == 0 ? timeleft.Seconds : timeleft.Minutes;

                    context.SetError("invalid_grant", string.Format("Your account is locked for {0} more {1}", timevalue, timetype));
                    return;
                }

                if (!(await userManager.CheckPasswordAsync(user, context.Password)))
                {
                    await userManager.AccessFailedAsync(user.Id);

                    if (await userManager.IsLockedOutAsync(user.Id))
                    {
                        context.SetError("invalid_grant", string.Format("Your account has been locked for {0} minutes", userManager.DefaultAccountLockoutTimeSpan.Minutes));
                        return;
                    }

                    var possibleAttempts = userManager.MaxFailedAccessAttemptsBeforeLockout;
                    var currentcount = await userManager.GetAccessFailedCountAsync(user.Id);

                    context.SetError("invalid_grant", string.Format("Invalid password. Your account will be locked after {0} more failed attempts.", possibleAttempts - currentcount));
                    return;

                }
                ClaimsIdentity oAuthIdentity = await userManager.CreateIdentityAsync(user,
                    context.Options.AuthenticationType);
                ClaimsIdentity cookiesIdentity = await userManager.CreateIdentityAsync(user,
                    CookieAuthenticationDefaults.AuthenticationType);

                var justCreatedIdentity = await userManager.FindByNameAsync(user.UserName);
                var roles = await userManager.GetRolesAsync(justCreatedIdentity.Id);

                AuthenticationProperties properties = CreateProperties(user.UserName, roles.ToArray(), user.EmailConfirmed);
                AuthenticationTicket ticket = new AuthenticationTicket(oAuthIdentity, properties);

                context.Validated(ticket);
                context.Request.Context.Authentication.SignIn(cookiesIdentity);
            }
        }

        /// <summary>
        /// Create the authentication properties
        /// Create the required properties that would be converted into Claims
        /// </summary>
        /// <param name="userName">The user name</param>
        /// <param name="roles">The user roles</param>
        /// <returns>The properties</returns>
        public static AuthenticationProperties CreateProperties(string userName, string[] roles, bool emailConfirmed)
        {
            IDictionary<string, string> data = new Dictionary<string, string>
            {
                { "userName", userName },
                { "roles", String.Join("," , roles) },
                { "emailConfirmed", emailConfirmed.ToString().ToLower()}
            };
            return new AuthenticationProperties(data);
        }
    }
}