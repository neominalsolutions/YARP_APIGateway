using API_Gateway.Recilency;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Polly.CircuitBreaker;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("https://localhost:5010", "https://localhost:5011");

builder.Services.AddControllers();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
            };
        });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Authenticated", policy => policy.RequireAuthenticatedUser());
});




var app = builder.Build();

app.MapGet("/api1", (HttpContext httpContext) =>
{
  Console.WriteLine(httpContext.Request.Headers["api1-request-header"]);

    return "API 1";
}).RequireAuthorization("Authenticated");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
