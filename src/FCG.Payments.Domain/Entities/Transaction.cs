using FCG.Payments.Domain.Enums;

namespace FCG.Payments.Domain.Entities;

public class Transaction
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public Guid GameId { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Transaction() { }

    public Transaction(Guid orderId, Guid userId, Guid gameId, decimal amount, PaymentStatus status)
    {
        Id = Guid.NewGuid();
        OrderId = orderId;
        UserId = userId;
        GameId = gameId;
        Amount = amount;
        Status = status;
        CreatedAt = DateTime.UtcNow;
    }
}
