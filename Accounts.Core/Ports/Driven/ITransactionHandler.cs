namespace Accounts.Core.Ports.Driven
{
    public interface ITransactionHandler
    {
        Task ExecuteAsync(Func<Task> operation);
    }
}