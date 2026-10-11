using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<MenuDbContext>
{
    public MenuDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<MenuDbContext>()
            .UseNpgsql("Host=localhost;Database=campuseats;Username=postgres;Password=placeholder;SSL Mode=Disable")
            .Options;
        return new MenuDbContext(options);
    }
}
