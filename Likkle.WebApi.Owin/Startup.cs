using System.Collections.Generic;
using System.IdentityModel.Tokens;
using IdentityServer3.AccessTokenValidation;
using Owin;

namespace Likkle.WebApi.Owin
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            JwtSecurityTokenHandler.InboundClaimTypeMap = new Dictionary<string, string>();

            // ensures that the api is only accessible if the access token 
            // provided by the TripGallery STS contains the gallerymanagement scope inside of it.
            // It happens in combination with the [Authorize] attribute on controller level.
            app.UseIdentityServerBearerTokenAuthentication(
             new IdentityServerBearerTokenAuthenticationOptions
             {
                 Authority = "https://boongalooidsrv.azurewebsites.net/identity",
                 RequiredScopes = new[] { "boongaloomanagement" }
             });

            var config = WebApiConfig.Register();

            app.UseWebApi(config);

            InitAutoMapper();
        }

        private void InitAutoMapper()
        {
            // Here we have to make the mapping between entities and DTOs
            //Mapper.Initialize(cfg => {
            //});
        }
    }
}