using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
var connectionString = builder.Configuration.GetConnectionString("Default");
if (string.IsNullOrWhiteSpace(connectionString))
    builder.Services.AddDbContext<MenuDbContext>(options => options.UseInMemoryDatabase("CampusEatsDev"));
else
    builder.Services.AddDbContext<MenuDbContext>(options => options.UseNpgsql(connectionString));
var configuredFrontendOrigin = builder.Configuration["FrontendOrigin"];
var allowedFrontendOrigins = new[]
{
    configuredFrontendOrigin,
    "https://campuseats-fullstack-frontend.vercel.app",
    "https://campuseats-frontend-git-main-daniduwijesinghes-projects.vercel.app",
    "http://localhost:5173"
}.Where(origin => !string.IsNullOrWhiteSpace(origin)).Select(origin => origin!).ToArray();

builder.Services.AddCors(options => options.AddPolicy("Frontend", policy =>
    policy.WithOrigins(allowedFrontendOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod()));

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MenuDbContext>();
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        db.Database.EnsureCreated();
        if (!db.MenuItems.Any())
            db.MenuItems.AddRange(MenuItem.Seed());
        db.SaveChanges();
    }
    else
    {
        db.Database.Migrate();
    }
}
app.UseCors("Frontend");
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));
app.Run();

public partial class Program { }

public class MenuItem
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public string Category { get; set; } = "";
    public bool Available { get; set; }

    public static MenuItem[] Seed() => new[]
    {
        new MenuItem { Id = 1, Name = "Chicken Rice", Price = 850m, Category = "Main", Available = true },
        new MenuItem { Id = 2, Name = "Vegetable Kottu", Price = 700m, Category = "Main", Available = true },
        new MenuItem { Id = 3, Name = "Fresh Lime Juice", Price = 250m, Category = "Drinks", Available = true }
    };
}

public sealed class MenuDbContext(DbContextOptions<MenuDbContext> options) : DbContext(options)
{
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MenuItem>().HasData(MenuItem.Seed());
    }
}

[ApiController]
[Route("api/menu")]
public sealed class MenuController : ControllerBase
{
    private readonly MenuDbContext _db;

    public MenuController(MenuDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MenuItem>>> GetAll() => Ok(await _db.MenuItems.AsNoTracking().ToListAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MenuItem>> GetById(int id)
    {
        var item = await _db.MenuItems.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<MenuItem>> Create(MenuItem item)
    {
        item.Id = 0;
        _db.MenuItems.Add(item);
        await _db.SaveChangesAsync();
        var created = item;
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Replace(int id, MenuItem item)
    {
        var existing = await _db.MenuItems.FindAsync(id);
        if (existing is null) return NotFound();
        existing.Name = item.Name; existing.Price = item.Price; existing.Category = item.Category; existing.Available = item.Available;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _db.MenuItems.FindAsync(id);
        if (existing is null) return NotFound();
        _db.MenuItems.Remove(existing);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
