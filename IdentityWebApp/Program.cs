using FluentValidation.AspNetCore;
using IdentityWebApp.Describers;
using IdentityWebApp.Models;
using IdentityWebApp.Requirements;
using IdentityWebApp.Validations;
using IdentityWebApp.Validations.FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<AppIdentityDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"));
});

builder.Services.AddTransient<IAuthorizationHandler, ExpireDateExchangeHandler>();

builder.Services.AddAuthorization(ops =>
{
    ops.AddPolicy("IstanbulPolicy", policy =>
    {
        policy.RequireClaim("city", "�stanbul");
    });

    ops.AddPolicy("ViolencePolicy", policy =>
    {
        policy.RequireClaim("violance"); 
    });

    ops.AddPolicy("ExchangePolicy", policy =>
    {
        policy.AddRequirements(new ExpireDateExchangeRequirement());
    });
});

builder.Services.AddAuthentication().AddFacebook(options =>
{
    // secrets.json
    options.AppId = builder.Configuration["Authentication:Facebook:AppId"];
    options.AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"];
}).AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
}).AddMicrosoftAccount(options =>
{
    options.ClientId = builder.Configuration["Authentication:Microsoft:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Microsoft:ClientSecret"];
});

builder.Services.AddIdentity<AppUser, AppRole>(ops =>
{
    ops.User.RequireUniqueEmail = true;
    ops.User.AllowedUserNameCharacters = "abc�defg�h�ijklmno�pqrs�tu�vwxyzABC�DEFG�HI�JKLMNO�PQRS�TU�VWXYZ0123456789-._@+";
    ops.Password.RequiredLength = 8;
}).AddPasswordValidator<CustomPasswordValidator>().AddUserValidator<CustomUserValidator>().AddErrorDescriber<CustomIdentityErrorDescriber>().AddEntityFrameworkStores<AppIdentityDbContext>().AddDefaultTokenProviders();

builder.Services.AddControllersWithViews()
    .AddFluentValidation((fv => fv.RegisterValidatorsFromAssemblyContaining<UserViewModelValidator>()));

CookieBuilder cookieBuilder = new CookieBuilder();

cookieBuilder.Name = "MyApp";
cookieBuilder.HttpOnly = false;
cookieBuilder.SameSite = SameSiteMode.Lax;
cookieBuilder.SecurePolicy = CookieSecurePolicy.SameAsRequest;

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = new PathString("/Home/Login");
    options.LogoutPath = new PathString("/Member/Logout");
    options.Cookie = cookieBuilder;
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
    options.AccessDeniedPath = new PathString("/Member/AccessDenied");
});

builder.Services.AddScoped<IClaimsTransformation, IdentityWebApp.ClaimProviders.ClaimProvider>();

builder.Services.AddMvc();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseDeveloperExceptionPage();
    app.UseStatusCodePages();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
