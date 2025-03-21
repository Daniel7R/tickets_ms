using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace TicketsMS.Infrastructure.Auth
{
    public static class WebApplicationsBuilderExtensions
    {
        public static WebApplicationBuilder AddAppAuthentication(this WebApplicationBuilder builder)
        {
            var settingsSection = builder.Configuration.GetSection("APISettings:JwtOptions");

            var secret = settingsSection.GetValue<string>("Secret");
            var issuer = settingsSection.GetValue<string>("Issuer");
            var audience = settingsSection.GetValue<string>("Audience");

            var key = Encoding.ASCII.GetBytes(secret);

            builder.Services.AddAuthentication(auth =>
            {
                auth.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                auth.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(auth =>
            {
                auth.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                };
            });

            return builder;
        }
    }
}
