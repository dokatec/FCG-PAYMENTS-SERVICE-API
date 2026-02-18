using MassTransit;
using FCG.Shared.Events;
using FCG.Payments.Domain.Interfaces;

namespace FCG.Payments.API.Consumers;

public class OrderPlacedConsumer : IConsumer<IOrderPlacedEvent>
{
    private readonly IPaymentService _paymentService; // Sua lógica que estava no Worker
    private readonly IPublishEndpoint _publishEndpoint; // Para avisar que aprovou

    public OrderPlacedConsumer(IPaymentService paymentService, IPublishEndpoint publishEndpoint)
    {
        _paymentService = paymentService;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Consume(ConsumeContext<IOrderPlacedEvent> context)
    {
        var order = context.Message;

        Console.WriteLine($"[PAYMENTS] Iniciando processamento da Ordem {order.OrderId}...");

        // 1. EXECUTA A LÓGICA QUE ESTAVA NO WORKER
        // Aqui você chama o método que valida saldo, salva no banco, etc.
        var success = await _paymentService.ProcessPaymentAsync(
         context.Message.OrderId,
         context.Message.UserId,
         context.Message.GameId,
         context.Message.Price
       );

        if (success)
        {
            Console.WriteLine($"[PAYMENTS] Pagamento APROVADO para a Ordem {order.OrderId}");

            // 2. DISPARA O EVENTO DE SUCESSO (Fase 4 - Coreografia)
            // O Games Service estará ouvindo isso para liberar o jogo
            await _publishEndpoint.Publish<IPaymentApprovedEvent>(new
            {
                OrderId = order.OrderId,
                UserId = order.UserId,
                GameId = order.GameId,
                Status = "Approved",
                ProcessedAt = DateTime.UtcNow
            });
        }
        else
        {
            Console.WriteLine($"[PAYMENTS] Pagamento REJEITADO para a Ordem {order.OrderId}");
            // Opcional: Publicar um evento de falha (IPaymentFailedEvent)
        }
    }
}