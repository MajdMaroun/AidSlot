using AidSlot.Data;
using AidSlot.Models;
using AidSlot.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Database + Identity setup
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireNonAlphanumeric = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// CSV import services
builder.Services.AddScoped<RecipientCsvParser>();
builder.Services.AddScoped<RecipientCsvValidator>();
builder.Services.AddScoped<RecipientMapper>();

// Time-slot service
builder.Services.AddScoped<TimeSlotAssignmentService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Seed roles on startup
using (var scope = app.Services.CreateScope())
{
    var roleManager =
        scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    string[] roles =
    {
        "Admin",
        "Coordinator",
        "Volunteer"
    };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // Explicit opt-in, development-only synthetic lab setup.
    if (app.Environment.IsDevelopment() &&
        Environment.GetEnvironmentVariable("AIDSLOT_LAB_SEED") == "SYNTHETIC_LOCAL_ONLY")
    {
        await LocalLabSeeder.SeedAsync(scope.ServiceProvider);
    }
}

app.Run();
