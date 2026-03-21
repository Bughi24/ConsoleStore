using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ConsoleStore.Data;
using ConsoleStore.Areas.Identity.Data;
using ConsoleStore.Service;
var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("ConsoleStoreContextConnection") ?? throw new InvalidOperationException("Connection string 'ConsoleStoreContextConnection' not found.");;

builder.Services.AddDbContext<ConsoleStoreContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("ConsoleStoreContextConnection")));
builder.Services.AddDefaultIdentity<IdentityUser>(option =>
{
    option.SignIn.RequireConfirmedAccount = false;
}) .AddRoles<IdentityRole>()
   .AddEntityFrameworkStores<ConsoleStoreContext>();


// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<TfIdfService>();
builder.Services.AddScoped<AutocompleteService>();
builder.Services.AddScoped<RleCompressionService>();
builder.Services.AddScoped<LuceneService>();



builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<ShoppingCart>(sp => ShoppingCart.GetCart(sp));
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await DbInitializer.SeedRolesAndAdminAsync(services);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseSession();


app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Store}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
