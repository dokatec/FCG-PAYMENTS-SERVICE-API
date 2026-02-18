using FCG.Payments.Domain.Entities;

namespace FCG.Payments.Domain.Interfaces;

public interface IPaymentRepository
{
    // Métodos de Persistência (já existentes)
    Task AddAsync(Transaction transaction);
    Task<int> SaveChangesAsync();

    // NOVOS: Métodos de Consulta para o Controller
    Task<IEnumerable<Transaction>> GetAllAsync();
    Task<Transaction?> GetByOrderIdAsync(Guid orderId);
    Task<IEnumerable<Transaction>> GetByUserIdAsync(Guid userId);
}