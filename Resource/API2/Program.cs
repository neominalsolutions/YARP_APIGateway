var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/api2", (HttpContext httpContext) =>
{
  Console.WriteLine("header2" + httpContext.Request.Headers["header2"]);
    return "API 2";
});

app.Run();
