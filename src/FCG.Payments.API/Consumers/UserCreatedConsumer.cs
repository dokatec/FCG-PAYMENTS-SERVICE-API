using MassTransit;
using FCG.Shared.Events; // Usando o contrato que criamos no Shared

namespace FCG.Payments.API.Consumers;

public class UserCreatedConsumer : IConsumer<IUserCreatedEvent>
{
    public async Task Consume(ConsumeContext<IUserCreatedEvent> context)
    {
        var message = context.Message;

        // Simulação de lógica de negócio: Criar uma carteira ou score para o novo usuário
        Console.WriteLine($"[CONSUMER] Mensagem recebida: Usuário {message.Name} (ID: {message.UserId})");

        // Aqui você faria o processamento real (ex: salvar no DB de pagamentos)
        await Task.CompletedTask;
    }
}