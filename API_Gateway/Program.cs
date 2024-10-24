using API_Gateway.Recilency;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Polly;
using Polly.CircuitBreaker;
using Polly.Extensions.Http;
using System.Text;
using Yarp.ReverseProxy.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
})
.AddPolicyHandler(RecilencyPolicyHelper.CreateRetryPolicy(3, TimeSpan.FromSeconds(2)))
.AddPolicyHandler(RecilencyPolicyHelper.CreateTimeoutPolicy(TimeSpan.FromSeconds(5)))
.AddPolicyHandler(RecilencyPolicyHelper.CreateCircuitBrakerPolicy(2, TimeSpan.FromSeconds(30))); // 2 kere üst üste hata olursa 30 saniye isteði kesintiye uðrat.

//.AddPolicyHandler(RecilencyPolicyHelper.CreateCircuitBrakerPolicy(2, TimeSpan.FromSeconds(10)));




var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseRateLimiter();

app.MapReverseProxy();

app.MapControllers();

app.Run();
