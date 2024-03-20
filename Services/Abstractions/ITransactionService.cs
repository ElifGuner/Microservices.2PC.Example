namespace Coordinator.Services.Abstractions
{
    public interface ITransactionService
    {
        Task<int> CreateTransactionAsync();
        Task PrepareServicesAsync(int transactionId);
        Task<bool> CheckReadyServicesAsync(int transactionId);
        Task CommitAsync(int transactionId);
        Task<bool> CheckTransactionStateServicesAsync(int transactionId);
        Task RollbackAsync(int transactionId);
    }
}
