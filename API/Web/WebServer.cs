﻿using System;
using System.Net.Http;
using System.Net;
using Microsoft.Owin.Security.OAuth;

#if WEBSERVER
using System.Threading;
using System.IO;
using OTA.Logging;
using Owin;
using System.Web.Http;
using Microsoft.Owin.Security.DataHandler;
using Microsoft.Owin.Security.DataHandler.Encoder;
using Microsoft.Owin.Security;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Security.DataHandler.Serializer;
using System.Security;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
#endif

/// <summary>
/// Basic OTA API's
/// </summary>
namespace OTA.Web.API
{
    /// <summary>
    /// Public access controllers.
    /// </summary>
    [AllowAnonymous]
    public class PublicController : ApiController
    {
        /// <summary>
        /// Get the online player names
        /// </summary>
        public System.Collections.Generic.IEnumerable<String> Get()
        { 
            return Terraria.Main.player
                .Where(x => x != null && x.active && !String.IsNullOrEmpty(x.Name))
                .Select(x => x.Name)
                .OrderBy(x => x);
        }
    }

    /// <summary>
    /// Player controller.
    /// </summary>
    [Authorize(Roles = "OTA.GetPlayers")]
    public class PlayerController : ApiController
    {
        public HttpResponseMessage Get(string name)
        {
            return this.Request.CreateResponse(HttpStatusCode.OK,
                Terraria.Main.player
                    .Where(x => x != null && x.active && x.Name == name)
                    .Select(x => new { Name = x.name, Position = x.position })
                .FirstOrDefault()
            );
        }
    }

    [Authorize(Roles = "OTA.User")]
    public class UserController : ApiController
    {
        public System.Collections.Generic.IEnumerable<object> Get()
        {
            var identity = User.Identity as ClaimsIdentity;

            return identity.Claims.Select(x => new
            {
                Type = x.Type,
                Value = x.Value
            });
        }
    }

}

namespace OTA.Web
{
    /// <summary>
    /// OTA web server for plugins. Currently based around OWIN
    /// </summary>
    public static class WebServer
    {

        #if WEBSERVER
        public static System.Web.Http.HttpConfiguration Config { get; private set; }

        public static string StaticFileDirectory = "Web";
        public static bool AllowInsecureHttp = true;
        public static string ServerKey = Guid.NewGuid().ToString();
        public static int SessionTimeoutHours = 12;

        internal static readonly AutoResetEvent Switch = new AutoResetEvent(false);

        static WebServer()
        {
            Config = new System.Web.Http.HttpConfiguration();

            Config.Routes.MapHttpRoute(
                name: "DefaultApi", 
                routeTemplate: "api/{controller}/{id}", 
                defaults: new { id = System.Web.Http.RouteParameter.Optional } 
            );

//            Config.DependencyResolver = new AssembliesResolver();
            Config.Services.Replace(typeof(System.Web.Http.Dispatcher.IAssembliesResolver), new PluginServiceResolver());

            Config.MapHttpAttributeRoutes();
//            Config.Formatters.Add(new System.Net.Http.Formatting.JsonMediaTypeFormatter());
        }
        #endif

        /// <summary>
        /// Start the web server at the specified address
        /// </summary>
        /// <param name="baseAddress">Base address.</param>
        public static void Start(string baseAddress)
        {
            #if WEBSERVER
            Switch.Reset();
            (new Thread(OWINServer.StartServer)).Start(baseAddress);
            #endif
        }

        /// <summary>
        /// Stop the server.
        /// </summary>
        public static void Stop()
        {
            #if WEBSERVER
            Switch.Set();
            #endif
        }
    }

    #if WEBSERVER
    class PluginServiceResolver : System.Web.Http.Dispatcher.DefaultAssembliesResolver
    {
        public override System.Collections.Generic.ICollection<System.Reflection.Assembly> GetAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies();
        }
    }

    class OWINServer
    {
        class PermissionsOAuthProvider : Microsoft.Owin.Security.OAuth.OAuthAuthorizationServerProvider
        {
            //            public override async System.Threading.Tasks.Task ValidateClientAuthentication(Microsoft.Owin.Security.OAuth.OAuthValidateClientAuthenticationContext context)
            //            {
            //                await Task.FromResult(context.Validated());
            ////                return base.ValidateClientAuthentication(context);
            //            }

            public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
            {
                context.Validated();
            }

            public override async Task GrantResourceOwnerCredentials(Microsoft.Owin.Security.OAuth.OAuthGrantResourceOwnerCredentialsContext context)
            {
                context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { "*" });
                if (context.UserName == "DeathCradle" && context.Password == "test")
                {
                    var identity = new ClaimsIdentity(context.Options.AuthenticationType);
                    identity.AddClaim(new Claim(ClaimTypes.Name, context.UserName));

                    //Load permissions for user
                    identity.AddClaim(new Claim(ClaimTypes.Role, "OTA.GetPlayers"));

//                    var ticket = new AuthenticationTicket(identity, new AuthenticationProperties()
//                        {
//                            IsPersistent = true,
//                            IssuedUtc = DateTime.UtcNow
//                        });
                    context.Validated(identity);
                }
                else
                {
                    context.SetError("invalid_grant", "The user name or password is incorrect.");
                    context.Rejected();
                }
            }
        }

        static OAuthAuthorizationServerOptions ServerOptions = new OAuthAuthorizationServerOptions()
        {
            TokenEndpointPath = new Microsoft.Owin.PathString("/token"),
            Provider = new PermissionsOAuthProvider(),

            AccessTokenExpireTimeSpan = TimeSpan.FromHours(WebServer.SessionTimeoutHours),

            AllowInsecureHttp = true, //WebServer.AllowInsecureHttp,

            /* Use app.SetDataProtectionProvider ? */
            RefreshTokenFormat = 
                    new SecureDataFormat<AuthenticationTicket>(DataSerializers.Ticket,
                new MonoDataProtector(WebServer.ServerKey), TextEncodings.Base64),
            AccessTokenFormat = 
                    new SecureDataFormat<AuthenticationTicket>(DataSerializers.Ticket,
                new MonoDataProtector(WebServer.ServerKey), TextEncodings.Base64),
            AuthorizationCodeFormat = 
                    new SecureDataFormat<AuthenticationTicket>(DataSerializers.Ticket,
                new MonoDataProtector(WebServer.ServerKey), TextEncodings.Base64),

            ApplicationCanDisplayErrors = true
        };

        static OAuthBearerAuthenticationOptions BearerOptions = new OAuthBearerAuthenticationOptions()
        {
            AccessTokenFormat =
                    new SecureDataFormat<AuthenticationTicket>(DataSerializers.Ticket,
                new MonoDataProtector(WebServer.ServerKey), TextEncodings.Base64),
            AuthenticationMode = AuthenticationMode.Active,
            AuthenticationType = "Bearer"
        };

        public void Configuration(Owin.IAppBuilder app)
        {
            app.UseErrorPage();
            app.UseOAuthAuthorizationServer(ServerOptions);
            app.UseOAuthBearerAuthentication(BearerOptions);

            app.UseWebApi(WebServer.Config);

//            app.SetDataProtectionProvider( TODO

            app.UseFileServer(new Microsoft.Owin.StaticFiles.FileServerOptions()
                {
                    RequestPath = new Microsoft.Owin.PathString("/web"),
                    FileSystem = new Microsoft.Owin.FileSystems.PhysicalFileSystem(WebServer.StaticFileDirectory),
                    EnableDirectoryBrowsing = false
                });

        }

        /// <summary>
        /// A mono compatible data protector for use with OWIN
        /// </summary>
        internal class MonoDataProtector : IDataProtector
        {
            private string[] _purposes;
            const String DefaultPurpose = "ota-web-dp";

            public MonoDataProtector()
            {
                _purposes = null;
            }

            //            public MonoDataProtector(string[] purposes)
            //            {
            //                _purposes = purposes;
            //            }

            public MonoDataProtector(params string[] purposes)
            {
                _purposes = purposes;
            }

            public byte[] Protect(byte[] data)
            {
                return System.Security.Cryptography.ProtectedData.Protect(data, this.GenerateEntropy(), DataProtectionScope.CurrentUser);
            }

            public byte[] Unprotect(byte[] data)
            {
                return System.Security.Cryptography.ProtectedData.Unprotect(data, this.GenerateEntropy(), DataProtectionScope.CurrentUser);
            }

            byte[] GenerateEntropy()
            {
                using (var hasher = SHA256.Create())
                {
                    using (var ms = new MemoryStream())
                    using (var cr = new CryptoStream(ms, hasher, CryptoStreamMode.Write))
                    using (var sw = new StreamWriter(cr))
                    {
                        //Default purpose 
                        sw.Write(DefaultPurpose);

                        if (_purposes != null) foreach (var purpose in _purposes)
                            {
                                sw.Write(purpose);
                            }
                    }

                    return hasher.Hash;
                }
            }
        }

        internal static void StartServer(object baseAddress)
        {
            System.Threading.Thread.CurrentThread.Name = "Web";

            if (!Directory.Exists(WebServer.StaticFileDirectory))
            {
                Directory.CreateDirectory(WebServer.StaticFileDirectory);
            }

            try
            {
                using (Microsoft.Owin.Hosting.WebApp.Start<OWINServer>(url: baseAddress as String))
                {
                    ProgramLog.Web.Log("Web server started listening on {0}", baseAddress);
                    WebServer.Switch.WaitOne();
                } 
            }
            catch (Exception e)
            {
                ProgramLog.Log(e);
            }
        }
    }
    #endif
}

