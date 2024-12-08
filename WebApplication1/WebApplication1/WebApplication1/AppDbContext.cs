using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;
using WebApplication1.DbModels;

namespace WebApplication1;
public class AppDbContext : DbContext
{
    public DbSet<Blog> Blogs { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Theme> Themes { get; set; }
    public DbSet<Post> Posts { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Blog>().ToTable("blogs");
    }
}