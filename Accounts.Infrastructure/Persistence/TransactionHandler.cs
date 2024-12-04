using Accounts.Core.Ports.Driven;

namespace Accounts.Infrastructure.Persistence
{
    public class TransactionHandler : ITransactionHandler
    {
        private readonly AppDbContext _dbContext;

        public TransactionHandler(AppDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task ExecuteAsync(Func<Task> transactionalOperation)
        {
            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    await transactionalOperation();

                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }
    }
}