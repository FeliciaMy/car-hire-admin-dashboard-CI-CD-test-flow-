using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Force the working directory
Directory.SetCurrentDirectory(builder.Environment.ContentRootPath);

// Ensure ContentRoot points to the project directory containing Views folder
string contentRoot = builder.Environment.ContentRootPath;
string viewsPath = Path.Combine(contentRoot, "Views");

// If Views folder doesn't exist in current directory, try to find it
if (!Directory.Exists(viewsPath))
{
    // Try going up directories to find the Views folder
    var current = AppContext.BaseDirectory;
    for (int i = 0; i < 6 && !string.IsNullOrEmpty(current); i++)
    {
        var testViewsPath = Path.Combine(current, "Views");
        if (Directory.Exists(testViewsPath))
        {
            contentRoot = current;
            builder.Environment.ContentRootPath = contentRoot;
            break;
        }
        current = Directory.GetParent(current)?.FullName;
    }
}

// Add services to the container
builder.Services.AddControllersWithViews();

// Configure DbContext with SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Session for custom authentication
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session timeout
    options.Cookie.HttpOnly = true; // Security: prevent client-side access
    options.Cookie.IsEssential = true; // Required for GDPR
});

var app = builder.Build();

// Seed initial data (Warehouses and Vacancies) if database is empty
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    // Ensure database exists
    db.Database.EnsureCreated();

    // Seed a couple of Warehouses if none exist )
    if (!db.Warehouses.Any())
    {
        var wh1 = new Warehouse { Name = "Cape Town DC", Address = "12 Logistics Ave, Montague Gardens, Cape Town" };
        var wh2 = new Warehouse { Name = "Johannesburg DC", Address = "45 Distribution Rd, Midrand, Johannesburg" };
        db.Warehouses.AddRange(wh1, wh2);
        db.SaveChanges();
    }

    // Seed Vacancies if none exist
    if (!db.Vacancies.Any())
    {
        // Use existing warehouse IDs
        var anyWarehouseId = db.Warehouses.Select(w => w.Id).First();
        var secondWarehouseId = db.Warehouses.OrderBy(w => w.Id).Skip(1).Select(w => w.Id).FirstOrDefault();

        var vacancies = new List<Vacancy>
        {
            new Vacancy
            {
                Name = "Delivery Driver - Day Shift",
                Description = "Deliver parcels across assigned routes. Valid driver's license required.",
                WarehouseId = anyWarehouseId
            },
            new Vacancy
            {
                Name = "Delivery Driver - Night Shift",
                Description = "Night deliveries with safety protocols. Overtime available.",
                WarehouseId = secondWarehouseId == 0 ? anyWarehouseId : secondWarehouseId
            },
            new Vacancy
            {
                Name = "Warehouse Picker/Packer",
                Description = "Pick, pack, and stage orders. Attention to detail and speed required.",
                WarehouseId = anyWarehouseId
            }
        };

        db.Vacancies.AddRange(vacancies);
        db.SaveChanges();
    }

    // Seed Vehicles 
    if (!db.Vehicles.Any())
    {
        var warehouseIds = db.Warehouses.OrderBy(w => w.Id).Select(w => w.Id).ToList();
        var primaryWarehouseId = warehouseIds.First();
        var secondaryWarehouseId = warehouseIds.Count > 1 ? warehouseIds[1] : primaryWarehouseId;

        var vehicles = new List<Vehicle>
        {
            new Vehicle { Make = "Toyota", Model = "Hilux", LicensePlate = "CA 123-456", WarehouseId = primaryWarehouseId },
            new Vehicle { Make = "Ford", Model = "Ranger", LicensePlate = "GP 987-654", WarehouseId = secondaryWarehouseId },
            new Vehicle { Make = "Nissan", Model = "NP200", LicensePlate = "CW 555-888", WarehouseId = primaryWarehouseId }
        };

        db.Vehicles.AddRange(vehicles);
        db.SaveChanges();
    }
}

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Enable session middleware (must be before Authorization)
app.UseSession();

app.UseAuthorization();

// Configure default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();