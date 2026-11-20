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

        if (query.DateFrom is { } from && query.DateTo is { } to && from > to)
        {
            errors.Add("dateTo", "A záró dátum nem lehet korábbi a kezdő dátumnál.");
        }

        if (query.Type is { } type && !Enum.IsDefined(type))
        {
            errors.Add("type", "A tranzakció típusa érvénytelen.");
        }

        if (query.CategoryId == Guid.Empty)
        {
            errors.Add("categoryId", "A kategória azonosítója érvénytelen.");
        }

        if (query.MinAmount is < 0)
        {
            errors.Add("minAmount", "A legkisebb összeg nem lehet negatív.");
        }

        if (query.MaxAmount is < 0)
        {
            errors.Add("maxAmount", "A legnagyobb összeg nem lehet negatív.");
        }

        if (query.MinAmount is { } min && query.MaxAmount is { } max && min > max)
        {
            errors.Add("maxAmount", "A legnagyobb összeg nem lehet kisebb a legkisebb összegnél.");
        }

        if (query.NormalizedSearch is { Length: > GetTransactionsQuery.MaxSearchLength })
        {
            errors.Add("search", $"A keresőszöveg legfeljebb {GetTransactionsQuery.MaxSearchLength} karakter lehet.");
        }

        return errors;
    }
}
