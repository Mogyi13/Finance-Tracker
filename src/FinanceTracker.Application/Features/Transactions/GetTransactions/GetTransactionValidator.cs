using FinanceTracker.Application.Common;
using FinanceTracker.Application.Common.Pagination;

namespace FinanceTracker.Application.Features.Transactions.GetTransactions;

internal static class GetTransactionsValidator
{
    public static FieldErrors Validate(GetTransactionsQuery query)
    {
        var errors = new FieldErrors();

        if (query.Page < 1)
        {
            errors.Add("page", "Az oldalszám legalább 1 lehet.");
        }

        if (query.PageSize < 1 || query.PageSize > GetTransactionsQuery.MaxPageSize)
        {
            errors.Add("pageSize", $"Az oldalméret 1 és {GetTransactionsQuery.MaxPageSize} között lehet.");
        }

        if (!Enum.IsDefined(query.SortBy))
        {
            errors.Add("sortBy", "A rendezési mező érvénytelen.");
        }

        if (!Enum.IsDefined(query.SortDirection))
        {
            errors.Add("sortDirection", "A rendezési irány érvénytelen.");
        }

        return errors;
    }
}
