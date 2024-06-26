using BostonScientificAVS.Services;
using Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.CodeAnalysis.Options;
using Microsoft.AspNetCore.Http;
using BostonScientificAVS.Websocket;
using System.Net.Sockets;

var builder = WebApplication.CreateBuilder(args);

var webSocketOptions = new WebSocketOptions()
{
    KeepAliveInterval = TimeSpan.FromSeconds(1000),
};
// Add services to the container.
builder.Services.AddControllersWithViews();

// Load the env file
DotNetEnv.Env.Load();

// get the expire time from env
int Time = DotNetEnv.Env.GetInt("EXPIRE_TIME");

// get the connection string from env
string DBConStr = DotNetEnv.Env.GetString("DB_CON_STRING");
builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseSqlServer(DBConStr);
});
builder.Services.AddScoped<ItemService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddSingleton<UdpClient>();
builder.Services.AddSingleton<TcpClient>();
/*builder.Services.AddSingleton<NetworkStream>()*/;
builder.Services.AddSingleton<IWebsocketHandler, WebsocketHandler>();


builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
}).AddCookie(option =>
{
    option.LoginPath = "/Login/Login";
    option.ExpireTimeSpan = TimeSpan.FromHours(Time);
});

//builder.Services.AddAuthentication("CustomScheme")
//        .AddScheme<AuthenticationSchemeOptions, CustomAuthenticationHandler>("CustomScheme", options => { });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseWebSockets(webSocketOptions);
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Login}/{id?}");

app.Run();
