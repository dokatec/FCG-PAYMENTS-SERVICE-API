using FCG.Payments.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCG.Payments.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Bloqueia acesso anônimo
public class PaymentController : ControllerBase
{
    private readonly IPaymentRepository _repository;

    public PaymentController(IPaymentRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")] // Apenas Admin vê tudo
    public async Task<IActionResult> GetAll()
    {
        var transactions = await _repository.GetAllAsync();
        return Ok(transactions);
    }

    [HttpGet("order/{orderId}")]
    public async Task<IActionResult> GetByOrder(Guid orderId)
    {
        var transaction = await _repository.GetByOrderIdAsync(orderId);
        if (transaction == null) return NotFound("Transação não encontrada.");

        return Ok(transaction);
    }
}