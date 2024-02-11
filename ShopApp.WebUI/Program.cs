using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ShopApp.Business.Abstract;
using ShopApp.Business.Concrete;
using ShopApp.DataAccess.Abstract;
using ShopApp.DataAccess.Concrete.EfCore;
using ShopApp.Entity;
using ShopApp.WebUI.EmailServices;
using ShopApp.WebUI.Extensions;
using ShopApp.WebUI.Identity;

//IConfiguration _configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

var builder = WebApplication.CreateBuilder(args);
var serviceProvider = builder.Services.BuildServiceProvider();
var configuration = serviceProvider.GetRequiredService<IConfiguration>();
// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<IProductService, ProductManager>();
builder.Services.AddScoped<ICategoryService, CategoryManager>();
builder.Services.AddScoped<ICartService, CartManager>();
builder.Services.AddScoped<IOrderService, OrderManager>();

builder.Services.AddDbContext<ApplicationContext>(options => options.UseSqlServer(configuration.GetConnectionString("MsSqlConnection")));
builder.Services.AddDbContext<ShopContext>(options => options.UseSqlServer(configuration.GetConnectionString("MsSqlConnection")));
builder.Services.AddIdentity<User, IdentityRole>().AddEntityFrameworkStores<ApplicationContext>().AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options =>
{
    //password
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = true;

    //lockout
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.AllowedForNewUsers = true;

    //options.User.AllowedUserNameCharacters = "";
    options.User.RequireUniqueEmail = false;
    options.SignIn.RequireConfirmedEmail = true;
    options.SignIn.RequireConfirmedPhoneNumber = false;
});
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/account/login";
    options.LogoutPath = "/account/logout";
    options.AccessDeniedPath = "/account/accessdenied";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(365);
    options.Cookie = new CookieBuilder
    {
        HttpOnly = true,
        Name = ".ShopApp.Security.Cookie",
        SameSite = SameSiteMode.Strict

    };
});

builder.Services.AddScoped<IEmailSender, SmtpEmailSender>(i=>
new SmtpEmailSender(
    builder.Configuration["EmailSender:Host"],
    builder.Configuration.GetValue<int>("EmailSender:Port"),
    builder.Configuration.GetValue<bool>("EmailSender:EnableSSL"),
    builder.Configuration["EmailSender:Username"],
    builder.Configuration["EmailSender:Password"])
    );

builder.Services.AddAuthentication().AddFacebook(opt =>
{
    opt.AppId = "330903299771918";
    opt.AppSecret = "5d882a0b2fa77894e438a8a9ed7f3994";
});
var app = builder.Build();
app.MigrateDatabase();

//Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    //    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseRouting();

app.UseAuthorization();

var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();

using (var scope = scopeFactory.CreateScope())

{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var cartService = scope.ServiceProvider.GetRequiredService<ICartService>();

    SeedIdentity.Seed(userManager, roleManager, cartService, configuration).Wait();
}

app.MapControllerRoute(
    name: "orders",
    pattern: "orders",
    defaults: new { controller = "Cart", action = "GetOrders" });
app.MapControllerRoute(
    name: "checkout",
    pattern: "checkout",
    defaults: new { controller = "Cart", action = "Checkout" });

app.MapControllerRoute(
    name: "cart",
    pattern: "cart",
    defaults: new { controller = "Cart", action = "Index" });

app.MapControllerRoute(
    name: "adminusers",
    pattern: "admin/user/list",
    defaults: new { controller = "Admin", action = "UserList" });

app.MapControllerRoute(
    name: "adminuseredit",
    pattern: "admin/user/{id?}",
    defaults: new { controller = "Admin", action = "UserEdit" });


app.MapControllerRoute(
    name: "adminroles",
    pattern: "admin/role/list",
    defaults: new { controller = "Admin", action = "RoleList" });
app.MapControllerRoute(
    name: "adminrolecreate",
    pattern: "admin/role/create",
    defaults: new { controller = "Admin", action = "RoleCreate" });
app.MapControllerRoute(
    name: "adminroleedit",
    pattern: "admin/role/{id?}",
    defaults: new { controller = "Admin", action = "RoleEdit" });
app.MapControllerRoute(
    name: "adminproducts",
    pattern: "admin/products",
    defaults: new { controller = "admin", action = "productlist" });

app.MapControllerRoute(
    name: "adminproductcreate",
    pattern: "admin/products/create",
    defaults: new { controller = "admin", action = "ProductCreate" });

app.MapControllerRoute(
    name: "adminproductedit",
    pattern: "admin/products/{id?}",
    defaults: new { controller = "admin", action = "ProductEdit" });

app.MapControllerRoute(
    name: "admincategories",
    pattern: "admin/categories",
    defaults: new { controller = "admin", action = "CategoryList" });

app.MapControllerRoute(
    name: "admincategorycreate",
    pattern: "admin/categories/create",
    defaults: new { controller = "admin", action = "CategoryCreate"});

app.MapControllerRoute(
    name: "admincategoryedit",
    pattern: "admin/categories/{id?}",
    defaults: new { controller = "admin", action = "CategoryEdit" });

app.MapControllerRoute(
    name: "search",
    pattern: "search",
    defaults: new { controller = "Shop", action = "search" });

app.MapControllerRoute(
    name: "products",
    pattern: "products/{category?}",
    defaults: new { controller = "Shop", action = "list" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "productdetails",
    pattern: "{url}",
    defaults: new { controller = "Shop", action = "details" });


app.Run();
