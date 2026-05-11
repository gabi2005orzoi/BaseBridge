using BaseBridge.Models;
using Microsoft.EntityFrameworkCore;

namespace BaseBridge.Database;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<EndpointData> Endpoints { get; set; }
    public DbSet<DbConfig> DbConfigs { get; set; }
    public DbSet<AiConfig> AiConfigs { get; set; }
    public DbSet<EndpointParameter> EndpointParameters { get; set; }
}