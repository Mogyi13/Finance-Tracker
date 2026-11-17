namespace FinanceTracker.Domain.Exceptions;

/// <summary>Domain invariáns megsértése. Az Application réteg validációja miatt normál működésben nem fordulhat elő.</summary>
public sealed class DomainException(string message) : Exception(message);
