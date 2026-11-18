using FinanceTracker.API.Infrastructure;
using FinanceTracker.Application.Common.Pagination;
using FinanceTracker.Application.Features.Transactions;
using FinanceTracker.Application.Features.Transactions.CreateTransaction;
using FinanceTracker.Application.Features.Transactions.DeleteTransaction;
using FinanceTracker.Application.Features.Transactions.GetTransaction;
using FinanceTracker.Application.Features.Transactions.GetTransactions;
using FinanceTracker.Application.Features.Transactions.UpdateTransaction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTracker.API.Controllers;

[ApiController]
[Authorize]
[Route("api/transactions")]
public class TransactionsController(
    CreateTransactionHandler create,
    GetTransactionsHandler list,
    GetTransactionHandler get,
    UpdateTransactionHandler update,
    DeleteTransactionHandler delete) : ControllerBase
{
    /// <summary>UC-06: a bejelentkezett felhasználó tranzakcióinak lapozott, rendezett listája.</summary>
    [HttpGet]
    [ProducesResponseType<PagedResult<TransactionDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll([FromQuery] GetTransactionsQuery query)
    {
        var result = await list.HandleAsync(query);
        return result.IsSuccess ? Ok(result.Value) : this.ToProblem(result.Error!);
    }

    /// <summary>Egy saját tranzakció részletei.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<TransactionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await get.HandleAsync(id);
        return result.IsSuccess ? Ok(result.Value) : this.ToProblem(result.Error!);
    }

    /// <summary>UC-05: új tranzakció rögzítése.</summary>
    [HttpPost]
    [ProducesResponseType<TransactionDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(CreateTransactionCommand command)
    {
        var result = await create.HandleAsync(command);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value)
            : this.ToProblem(result.Error!);
    }

    /// <summary>UC-07: tranzakció módosítása.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType<TransactionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, UpdateTransactionCommand command)
    {
        var result = await update.HandleAsync(id, command);
        return result.IsSuccess ? Ok(result.Value) : this.ToProblem(result.Error!);
    }

    /// <summary>UC-08: tranzakció törlése.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await delete.HandleAsync(id);
        return result.IsSuccess ? NoContent() : this.ToProblem(result.Error!);
    }
}
