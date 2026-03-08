using Microsoft.EntityFrameworkCore;

namespace MudBlazorWeb.Infrastructure.Database.Postgres;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }


}
