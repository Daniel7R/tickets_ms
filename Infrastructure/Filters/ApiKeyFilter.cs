using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TicketsMS.Application.DTOs.Request;

namespace TicketsMS.Infrastructure.Filters
{
    public class ApiKeyFilter : Attribute, IAuthorizationFilter
    {
        private const string API_KEY_HEADER_NAME = "x-api-key";
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var request = context.HttpContext.Request;

            var config = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            var EXPECTED = config["APIKEY:TICKETS"];
            if (string.IsNullOrEmpty(EXPECTED) || !request.Headers.TryGetValue(API_KEY_HEADER_NAME, out var provided) || provided != EXPECTED)
                context.Result = new UnauthorizedObjectResult(new ResponseDTO<string?>{ Message="API KEY is missing or invalid" });
        }
    }
}
