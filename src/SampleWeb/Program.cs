using Microsoft.AspNetCore;

var builder = WebHost.CreateDefaultBuilder();
builder.UseContentRoot(Directory.GetCurrentDirectory());
builder.UseStartup<Startup>();
var webHost = builder.Build();
webHost.Run();