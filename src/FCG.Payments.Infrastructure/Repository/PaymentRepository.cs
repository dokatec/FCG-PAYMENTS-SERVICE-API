using FCG.Payments.Domain.Entities;
using FCG.Payments.Domain.Interfaces;
using FCG.Payments.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FCG.Payments.Infrastructure.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly PaymentsDbContext _context;

    public PaymentRepository(PaymentsDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Transaction transaction)
    {
        await _context.Transactions.AddAsync(transaction);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    // Busca todos para o Admin
    public async Task<IEnumerable<Transaction>> GetAllAsync()
    {
        return await _context.Transactions
            .AsNoTracking()
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    // Busca por OrderId (ID da Ordem vinda do Games Service)
    public async Task<Transaction?> GetByOrderIdAsync(Guid orderId)
    {
        return await _context.Transactions
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.OrderId == orderId);
    }

    // Busca todas as transações de um usuário específico
    public async Task<IEnumerable<Transaction>> GetByUserIdAsync(Guid userId)
    {
        return await _context.Transactions
            .AsNoTracking()
            .Where(t => t.UserId == userId)
            .ToListAsync();
    }
}