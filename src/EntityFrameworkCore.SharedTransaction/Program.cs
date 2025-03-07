namespace EntityFrameworkCore.SharedTransaction; 
using EntityFrameworkCore.SharedTransaction.Database;
using EntityFrameworkCore.SharedTransaction.Database.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

internal class Program
{
    static async Task Main(string[] _)
    {
        var parentOptions = new DbContextOptionsBuilder()
            .UseSqlServer(DatabaseContext.ConnectionString)
            .Options;

        using (var parentContext = new DatabaseContext(parentOptions))
        {
            if (parentContext.Database.GetPendingMigrations().Any())
            {
                await parentContext.Database.MigrateAsync();
            }

            using (var transaction = parentContext.Database.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                var initial = new Entity() { Name = Guid.NewGuid().ToString() };

                await parentContext.Set<Entity>().AddAsync(initial);

                await parentContext.SaveChangesAsync();

                int entityId = initial.Id;

                var childOptions = new DbContextOptionsBuilder()
                    .UseSqlServer(parentContext.Database.GetDbConnection())
                    .Options;

                using (var childContext = new DatabaseContext(childOptions))
                {
                    await childContext.Database.UseTransactionAsync(transaction.GetDbTransaction());

                    var reloaded = await childContext.Set<Entity>().SingleOrDefaultAsync(x => x.Id == entityId);

                    reloaded.Name = $"Name {reloaded.Id}";

                    await childContext.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                //await transaction.RollbackAsync();
            }
        }
    }
}
