using Microsoft.Owin;
using Microsoft.Owin.Security.OAuth;
using Microsoft.Owin.Security.Cookies;
using Owin;
using System;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Owin.Cors;
using System.Web.Cors;
using System.Configuration;

[assembly: OwinStartup(typeof(GOChatAPI.Startup1))]

namespace GOChatAPI
{
    public class Startup1
    {
        public void Configuration(IAppBuilder app)
        {
            // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=316888
            //enable cors origin requests

            var corsPolicy = new CorsPolicy()
            {
                AllowAnyHeader = true,
                AllowAnyMethod = true,
                SupportsCredentials = true
            };

            corsPolicy.Origins.Add("http://localhost:3000");
            corsPolicy.Origins.Add("http://localhost:3001");
            corsPolicy.Origins.Add("https://gochat-tau.vercel.app");
            corsPolicy.Origins.Add("https://gochat.goit.net.ng");
            corsPolicy.Origins.Add("http://gochat.h3azh2avb9e7aghq.centralus.azurecontainer.io:3000");

            var policyProvider = new CorsPolicyProvider()
            {
                PolicyResolver = (context) => Task.FromResult(corsPolicy)
            };
            var corsOptions = new CorsOptions()
            {
                PolicyProvider = policyProvider
            };

            app.UseCors(corsOptions);

            var myProvider = new MyAuthorizationServerProvider();
            var refreshTokenProvider = new RefreshTokenProvider();
            OAuthAuthorizationServerOptions options = new OAuthAuthorizationServerOptions
            {
                AllowInsecureHttp = true,
                TokenEndpointPath = new PathString("/token"),
                AccessTokenExpireTimeSpan = TimeSpan.FromMinutes(5),
                Provider = myProvider,
                RefreshTokenProvider = refreshTokenProvider
            };
            app.UseOAuthAuthorizationServer(options);
            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());

            HttpConfiguration config = new HttpConfiguration();
            WebApiConfig.Register(config);
        }
    }
}
