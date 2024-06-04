namespace DropoutCoder.EntityFramework.SharedTransaction
{
    using DropoutCoder.EntityFramework.SharedTransaction.Database;
    using DropoutCoder.EntityFramework.SharedTransaction.Database.Schema;
    using System;
    using System.Data;
    using System.Data.Entity;
    using System.Threading.Tasks;

    internal class Program
    {
        static async Task Main(string[] args)
        {

            using (var parentContext = new DatabaseContext(DatabaseContext.ConnectionString))
            {
                if (parentContext.Database.CreateIfNotExists())
                {
                    parentContext.Database.Initialize(true);
                }

                using (var transaction = parentContext.Database.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    var parent = new Entity() { Name = Guid.NewGuid().ToString() };

                    parentContext.Set<Entity>().Add(parent);

                    await parentContext.SaveChangesAsync();

                    int entityId = parent.Id;

                    using (var childContext = new DatabaseContext(parentContext.Database.Connection, contextOwnsConnection: false))
                    {
                        childContext.Database.UseTransaction(transaction.UnderlyingTransaction);

                        var reloaded = await childContext.Set<Entity>().SingleOrDefaultAsync(e => e.Id == entityId);

                        reloaded.Name = $"Name {reloaded.Id}";

                        await childContext.SaveChangesAsync();
                    }

                    transaction.Commit();
                    //transaction.Rollback();
                }
            }
        }
    }
}
