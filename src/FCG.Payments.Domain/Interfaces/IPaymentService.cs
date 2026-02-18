namespace FCG.Payments.Domain.Interfaces;

public interface IPaymentService
{
    // O método que o Consumer vai chamar ao receber a mensagem do RabbitMQ
    Task<bool> ProcessPaymentAsync(Guid orderId, Guid userId, Guid gameId, decimal amount);
}