using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Polly;
using Polly.Extensions.Http;
using System.Text;
using Yarp.ReverseProxy.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddReverseProxy()
.LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// http://jwtbuilder.jamiekurtz.com/

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

builder.Services.AddRateLimiter(rateLimiterOptions =>
{
    rateLimiterOptions.AddFixedWindowLimiter("fixed", options =>
    {
        options.Window = TimeSpan.FromSeconds(10);
        options.PermitLimit = 5;
       
    });

  rateLimiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
 
});

builder.Services.AddHttpClient("api1", opt =>
{
  opt.BaseAddress = new Uri("https://localhost:5010");
}).AddPolicyHandler(policy => HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, retryAttempt =>
            {
              Console.WriteLine("Before Retry");
              return TimeSpan.FromSeconds(2);
            }, onRetry: (outcome, timespan, retryCount, context) =>
            {
              Console.WriteLine("Yeniden Deneniyor...");
            })) // 2 saniyede 1 3 kez dene
.AddPolicyHandler(policy =>
{
  return Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(5),(context,timespan,task) =>
  {
    Console.WriteLine("Zaman aþýmýna uðradý");

    return Task.CompletedTask;
  });
}); // 5 saniye sonra zaman aþýmýna düþ.

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.UseRateLimiter();

app.MapReverseProxy();

app.MapControllers();

app.Run();
