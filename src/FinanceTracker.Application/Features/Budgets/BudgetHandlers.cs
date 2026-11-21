using FinanceTracker.Application.Common;
using FinanceTracker.Application.Common.Abstractions;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Exceptions;

namespace FinanceTracker.Application.Features.Budgets;

/// <summary>UC-26: a felhasználó költségkereteinek listája.</summary>
public sealed class GetBudgetsHandler(ICurrentUser currentUser, IBudgetQueries queries)
{
    public async Task<Result<IReadOnlyList<BudgetDto>>> HandleAsync()
    {
        if (currentUser.UserId is not { } userId)
        {
            return Error.Unauthorized("A művelethez bejelentkezés szükséges.");
        }

        return Result<IReadOnlyList<BudgetDto>>.Ok(await queries.GetAllAsync(userId));
    }
}

/// <summary>UC-26: költségkeret létrehozása.</summary>
public sealed class CreateBudgetHandler(
    ICurrentUser currentUser,
    ICategoryRepository categories,
    IBudgetRepository budgets,
    IBudgetQueries queries,
    TimeProvider timeProvider)
{
    public async Task<Result<BudgetDto>> HandleAsync(SaveBudgetCommand command)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Error.Unauthorized("A művelethez bejelentkezés szükséges.");
        }

        var (valid, error) = await BudgetRules.ValidateAsync(command, userId, categories, budgets, excludeBudgetId: null);
        if (error is not null)
        {
            return error;
        }

        Budget budget;
        try
        {
            budget = Budget.Create(Guid.NewGuid(), userId, valid!.CategoryId, valid.Amount, valid.Currency!, valid.PeriodType,
                valid.StartDate, valid.EndDate, timeProvider.GetUtcNow().UtcDateTime);
        }
        catch (DomainException ex)
        {
            return Error.Failure(ex.Message);
        }

        await budgets.AddAsync(budget);
        return Result<BudgetDto>.Ok((await queries.GetAllAsync(userId)).First(b => b.Id == budget.Id));
    }
}

/// <summary>UC-26: költségkeret módosítása.</summary>
public sealed class UpdateBudgetHandler(
    ICurrentUser currentUser,
    ICategoryRepository categories,
    IBudgetRepository budgets,
    IBudgetQueries queries,
    TimeProvider timeProvider)
{
    public async Task<Result<BudgetDto>> HandleAsync(Guid id, SaveBudgetCommand command)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Error.Unauthorized("A művelethez bejelentkezés szükséges.");
        }

        var budget = await budgets.GetByIdAsync(userId, id);
        if (budget is null)
        {
            return Error.NotFound("A költségkeret nem található.");
        }

        var (valid, error) = await BudgetRules.ValidateAsync(command, userId, categories, budgets, excludeBudgetId: id);
        if (error is not null)
        {
            return error;
        }

        try
        {
            budget.Update(valid!.CategoryId, valid.Amount, valid.Currency!, valid.PeriodType, valid.StartDate, valid.EndDate,
                timeProvider.GetUtcNow().UtcDateTime);
        }
        catch (DomainException ex)
        {
            return Error.Failure(ex.Message);
        }

        await budgets.UpdateAsync(budget);
        return Result<BudgetDto>.Ok((await queries.GetAllAsync(userId)).First(b => b.Id == budget.Id));
    }
}

/// <summary>UC-26: költségkeret törlése (a tranzakciókat nem érinti).</summary>
public sealed class DeleteBudgetHandler(ICurrentUser currentUser, IBudgetRepository budgets)
{
    public async Task<Result<Unit>> HandleAsync(Guid id)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Error.Unauthorized("A művelethez bejelentkezés szükséges.");
        }

        var budget = await budgets.GetByIdAsync(userId, id);
        if (budget is null)
        {
            return Error.NotFound("A költségkeret nem található.");
        }

        await budgets.DeleteAsync(budget);
        return Result<Unit>.Ok(Unit.Value);
    }
}

/// <summary>UC-27: a költségkeretek felhasználtsága és figyelmeztetései a mai napra (vagy a megadott napra).</summary>
public sealed class GetBudgetStatusHandler(ICurrentUser currentUser, IBudgetQueries queries, BudgetSettings settings, TimeProvider timeProvider)
{
    public async Task<Result<BudgetStatusesDto>> HandleAsync(DateOnly? asOf = null)
    {
        if (currentUser.UserId is not { } userId)
        {
            return Error.Unauthorized("A művelethez bejelentkezés szükséges.");
        }

        var today = asOf ?? DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
        var budgets = await queries.GetAllAsync(userId);

        var resolved = budgets
            .Select(b => (Budget: b, Period: BudgetPeriods.Resolve(b.PeriodType, b.StartDate, b.EndDate, today)))
            .ToList();

        var spent = resolved.Count == 0
            ? new Dictionary<Guid, decimal>()
            : await queries.GetSpentAsync(userId, resolved
                .Select(r => new BudgetSpendRequest(r.Budget.Id, r.Budget.CategoryId, r.Budget.Currency, r.Period.Start, r.Period.End))
                .ToList());

        var items = resolved.Select(r =>
        {
            var used = spent.TryGetValue(r.Budget.Id, out var s) ? s : 0m;
            var (remaining, ratio, level) = BudgetStatusCalculator.Calculate(r.Budget.Amount, used, settings.WarningThreshold);
            return new BudgetStatusDto(r.Budget.Id, r.Budget.CategoryId, r.Budget.CategoryName, r.Budget.Currency, r.Budget.PeriodType,
                r.Period.Start, r.Period.End, r.Period.IsActive, r.Budget.Amount, used, remaining, ratio, level);
        })
        // A legkritikusabb elöl: túllépett, kimerített, figyelmeztetett, majd a többi; azon belül a nagyobb felhasználtság.
        .OrderByDescending(i => i.IsActive)
        .ThenByDescending(i => i.Level)
        .ThenByDescending(i => i.UsedRatio)
        .ThenBy(i => i.CategoryName, StringComparer.CurrentCultureIgnoreCase)
        .ToList();

        return Result<BudgetStatusesDto>.Ok(new BudgetStatusesDto(
            today,
            settings.WarningThreshold,
            items,
            items.Count(i => i.IsActive && i.Level == BudgetLevel.Warning),
            items.Count(i => i.IsActive && i.Level is BudgetLevel.Exceeded or BudgetLevel.Reached)));
    }
}
