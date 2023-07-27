using BostonScientificAVS.Services;
using Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Load the env file
DotNetEnv.Env.Load();

// get the expire time from env
int Time = DotNetEnv.Env.GetInt("EXPIRE_TIME");
builder.Services.AddAuthentication(
    CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(option =>
    {
        option.LoginPath = "/Login/Login";
        option.ExpireTimeSpan = TimeSpan.FromMinutes(Time);
    });

// get the connection string from env
string DBConStr = DotNetEnv.Env.GetString("DB_CON_STRING");
builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseSqlServer(DBConStr);
});
builder.Services.AddScoped<ItemService>();
builder.Services.AddScoped<UserService>();

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

app.UseRouting();
app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Login}/{id?}");

app.Run();
