using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApplication1.Data.DBContexts;
using WebApplication1.Data.Entities;

namespace WebApplication1.Middleware.Auth
{
    public class AuthTokenMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next;

        public async Task InvokeAsync(HttpContext context, DataContext dataContext, IConfiguration configuration)
        {
            Guid tokenId = default;
            string authMessage = "";
            string authHeader = context.Request.Headers.Authorization.ToString();
            if (string.IsNullOrEmpty(authHeader))
            {
                authMessage = "Missing 'Authorization' Header";
            }
            else
            {
                string authScheme = "Bearer ";
                if (!authHeader.StartsWith(authScheme))
                {
                    authMessage = $"Помилка схеми авторизації: потрібна '{authScheme}'.";
                }
                else
                {
                    string credentials = authHeader[authScheme.Length..];
                    try { tokenId = Guid.Parse(credentials); }
                    catch { authMessage = "Invalid credentials format UUID expected"; }

                }
            }
            if (authMessage == "")
            {
                AuthToken? authToken = dataContext
                    .AuthTokens
                    .Include(at => at.UserAccess)
                        .ThenInclude(ua => ua.User)
                    .FirstOrDefault(t => t.Jti == tokenId);
                if (authToken == null)
                {
                    authMessage = "Access Token rejected";
                }
                else if (authToken.Exp <= DateTime.Now)
                {
                    authMessage = "Access Token expired";
                }
                else
                {
                    int lifetime = configuration.GetSection("AuthToken").GetSection("Lifetime").Get<int>();
                    authToken.Exp.AddSeconds(lifetime);
                    var saveChangesTask = dataContext.SaveChangesAsync();
                    User user = authToken.UserAccess.User;

                    context.User = new System.Security.Claims.ClaimsPrincipal(
                        new ClaimsIdentity(
                            [
                                new Claim(ClaimTypes.Sid, user.Id.ToString()),
                                     new Claim(ClaimTypes.Name, user.Name),
                                     new Claim(ClaimTypes.Email, user.Email),
                                     new Claim(ClaimTypes.NameIdentifier, user.Slug!),
                            ],
                            nameof(AuthTokenMiddleware)
                        )
                    );
                    await saveChangesTask;
                }
            }
            context.Items.Add(nameof(AuthTokenMiddleware), authMessage);
            await _next(context);
        }
    }

    public static class AuthTokenMiddlewareExtensions
    {
        public static IApplicationBuilder UseAuthToken(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthTokenMiddleware>();
        }
    }
}
