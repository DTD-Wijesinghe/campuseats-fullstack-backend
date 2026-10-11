using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors(options => options.AddPolicy("Frontend", policy =>
    policy.WithOrigins(builder.Configuration["FrontendOrigin"] ?? "http://localhost:5173")
        .AllowAnyHeader()
        .AllowAnyMethod()));

var app = builder.Build();
app.UseCors("Frontend");
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));
app.Run();

public partial class Program { }

public record MenuItem(int Id, string Name, decimal Price, string Category, bool Available);

[ApiController]
[Route("api/menu")]
public sealed class MenuController : ControllerBase
{
    private static readonly List<MenuItem> Items = new()
    {
        new(1, "Chicken Rice", 850m, "Main", true),
        new(2, "Vegetable Kottu", 700m, "Main", true),
        new(3, "Fresh Lime Juice", 250m, "Drinks", true)
    };

    [HttpGet]
    public ActionResult<IEnumerable<MenuItem>> GetAll() => Ok(Items);

    [HttpGet("{id:int}")]
    public ActionResult<MenuItem> GetById(int id)
    {
        var item = Items.FirstOrDefault(x => x.Id == id);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public ActionResult<MenuItem> Create(MenuItem item)
    {
        var created = item with { Id = Items.Count == 0 ? 1 : Items.Max(x => x.Id) + 1 };
        Items.Add(created);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public IActionResult Replace(int id, MenuItem item)
    {
        var index = Items.FindIndex(x => x.Id == id);
        if (index < 0) return NotFound();
        Items[index] = item with { Id = id };
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        var removed = Items.RemoveAll(x => x.Id == id);
        return removed == 0 ? NotFound() : NoContent();
    }
}
