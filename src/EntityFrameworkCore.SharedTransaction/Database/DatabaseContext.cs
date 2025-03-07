namespace EntityFrameworkCore.SharedTransaction.Database; 
using EntityFrameworkCore.SharedTransaction.Database.Schema;
using Microsoft.EntityFrameworkCore;

public class DatabaseContext : DbContext
{
    public static string ConnectionString { get; } = "Server=(localdb)\\MSSQLLocalDB;Database=EntityFrameworkCore;Integrated Security=true;";

    public DatabaseContext(DbContextOptions options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder
            .Entity<Entity>();

        entity
            .HasKey(e => e.Id)
            .IsClustered();

        entity
            .Property(e => e.Name)
            .HasMaxLength(128)
            .IsRequired();

        base
            .OnModelCreating(modelBuilder);
    }
}
