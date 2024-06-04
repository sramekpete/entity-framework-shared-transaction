namespace DropoutCoder.EntityFramework.SharedTransaction.Database
{
    using DropoutCoder.EntityFramework.SharedTransaction.Database.Schema;
    using System.Data.Common;
    using System.Data.Entity;

    public class DatabaseContext : DbContext
    {
        public static string ConnectionString { get; } = "Server=(localdb)\\MSSQLLocalDB;Database=EntityFramework;Integrated Security=true;";

        public DatabaseContext(string connectionString)
            : base(connectionString) { }

        public DatabaseContext(DbConnection dbConnection, bool contextOwnsConnection)
            : base(dbConnection, contextOwnsConnection) { }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            var entity = modelBuilder
                .Entity<Entity>();

            entity
                .HasKey(e => e.Id);

            entity
                .Property(e => e.Name)
                .HasMaxLength(128)
                .IsRequired();

            base.OnModelCreating(modelBuilder);
        }
    }
}
