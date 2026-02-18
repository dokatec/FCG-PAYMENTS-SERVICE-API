using FCG.Payments.Domain.Interfaces;
using FCG.Payments.Domain.Entities;
using Microsoft.Extensions.Logging;
using FCG.Payments.Domain.Enums;

namespace FCG.Payments.Application.Services;

public class PaymentService : IPaymentService

{
    private readonly IPaymentRepository _repository;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(IPaymentRepository repository, ILogger<PaymentService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<bool> ProcessPaymentAsync(Guid orderId, Guid userId, Guid gameId, decimal amount)
    {
        try
        {
            _logger.LogInformation("[PAGAMENTO] Iniciando gravação da transação para Ordem {OrderId}", orderId);

            // 1. Simulação de Regra de Negócio (ex: Aluno tem saldo?)
            if (amount <= 0)
            {
                _logger.LogWarning("[PAGAMENTO] Valor inválido para a Ordem {OrderId}", orderId);
                return false;
            }

            // 2. Criar o objeto de transação
            var transaction = new Transaction(
            orderId,
            userId,
            gameId,
            amount,
            PaymentStatus.Approved);

            // 3. Persistência no Banco de Dados (O que estava faltando!)
            await _repository.AddAsync(transaction);
            // 4. Commit da transação no banco
            var rowsAffected = await _repository.SaveChangesAsync();

            if (rowsAffected > 0)
            {
                _logger.LogInformation("[PAGAMENTO] Transação gravada no banco com sucesso para a Ordem {OrderId}", orderId);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[PAGAMENTO] Erro crítico ao gravar transação da Ordem {OrderId}", orderId);
            return false;
        }
    }
}