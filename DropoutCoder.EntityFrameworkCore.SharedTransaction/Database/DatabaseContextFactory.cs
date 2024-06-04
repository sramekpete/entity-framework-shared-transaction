namespace DropoutCoder.EntityFrameworkCore.SharedTransaction.Database
{
    using DropoutCoder.EntityFramework.SharedTransaction.Database;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;

    public class DatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
    {
        public DatabaseContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>()
                .UseSqlServer(DatabaseContext.ConnectionString);

            return new DatabaseContext(optionsBuilder.Options);
        }
    }
}
