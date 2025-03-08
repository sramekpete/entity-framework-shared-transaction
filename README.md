# DbContext transaction sharing

Sample implementation of sharing transaction between two instances of the same DBContext for EntityFramework and EntityFrameworkCore.

## EntityFramework

Using older EntityFramework you can achieve transaction sharing by creating new transaction using initial DbContext, reusing `DbConnection` to initialize new `DbContext` and subsequently calling `UseTransaction` method to instruct `Database` to use desired transaction.

```csharp
using (var parentContext = new DatabaseContext("...connection string...")) {
    // Create transaction calling BeginTransaction method
    using (var transaction = parentContext.Database.BeginTransaction(IsolationLevel.ReadCommitted)) {
        var parent = new Entity() { Name = Guid.NewGuid().ToString() };

        parentContext.Set<Entity>().Add(parent);

        await parentContext.SaveChangesAsync();

        int entityId = parent.Id;

        // Create child DbContext using existing database connection without disposing it by passing false as contextOwnsConnection parameter
        using (var childContext = new DatabaseContext(parentContext.Database.Connection, contextOwnsConnection: false)) {
            // Instruct DbContext to use existing transaction
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

### Related info

[Connection management - EF6](https://learn.microsoft.com/en-us/ef/ef6/fundamentals/connection-management)

## EntityFrameworkCore

```csharp
var parentOptions = new DbContextOptionsBuilder()
            .UseSqlServer("...connection string...")
            .Options;

using (var parentContext = new DatabaseContext(parentOptions))
{
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
