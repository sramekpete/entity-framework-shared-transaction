# DbContext transaction sharing

Sample implementation of sharing transaction between two instances of the same DBContext for EntityFramework and EntityFrameworkCore.

## EntityFramework 

```csharp
using (var parentContext = new DatabaseContext("...connection string...")) {
    if (parentContext.Database.CreateIfNotExists()) {
        parentContext.Database.Initialize(true);
    }

    // Create transaction calling BeginTransaction method
    using (var transaction = parentContext.Database.BeginTransaction(IsolationLevel.ReadCommitted)) {
        var parent = new Entity() { Name = Guid.NewGuid().ToString() };

        parentContext.Set<Entity>().Add(parent);

        await parentContext.SaveChangesAsync();

        int entityId = parent.Id;

        // Create child DbContext using existing database connection without disposing it by passing false as contextOwnsConnection parameter
        using (var childContext = new DatabaseContext(parentContext.Database.Connection, contextOwnsConnection: false)) {
            childContext.Database.UseTransaction(transaction.UnderlyingTransaction);

            var reloaded = await childContext.Set<Entity>().SingleOrDefaultAsync(e => e.Id == entityId);

            reloaded.Name = $"Name {reloaded.Id}";

            await childContext.SaveChangesAsync();
        }

        transaction.Commit();
        //transaction.Rollback();
    }
}
```

## EntityFrameworkCore

```csharp
var parentOptions = new DbContextOptionsBuilder()
            .UseSqlServer(DatabaseContext.ConnectionString)
            .Options;

using (var parentContext = new DatabaseContext(parentOptions))
{
    if (parentContext.Database.GetPendingMigrations().Any())
    {
        await parentContext.Database.MigrateAsync();
    }

    using (var transaction = parentContext.Database.BeginTransaction(IsolationLevel.ReadCommitted)) {
        var initial = new Entity() { Name = Guid.NewGuid().ToString() };

        await parentContext.Set<Entity>().AddAsync(initial);

        await parentContext.SaveChangesAsync();

        int entityId = initial.Id;

        var childOptions = new DbContextOptionsBuilder()
            .UseSqlServer(parentContext.Database.GetDbConnection())
            .Options;

        using (var childContext = new DatabaseContext(childOptions)) {
            await childContext.Database.UseTransactionAsync(transaction.GetDbTransaction());

            var reloaded = await childContext.Set<Entity>().SingleOrDefaultAsync(x => x.Id == entityId);

            reloaded.Name = $"Name {reloaded.Id}";

            await childContext.SaveChangesAsync();
        }

        await transaction.CommitAsync();
        //await transaction.RollbackAsync();
    }
}

```