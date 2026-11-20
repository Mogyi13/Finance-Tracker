using FinanceTracker.Application.Features.Categorization.Ml;
using FinanceTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Infrastructure.Persistence.Repositories;

public sealed class CategorizationFeedbackRepository(ApplicationDbContext db) : ICategorizationFeedbackRepository
{
    public async Task SaveCorrectionAsync(Transaction transaction, CategorizationFeedback feedback)
    {
        // A tranzakciót ugyanez a DbContext töltötte be, ezért követett; ha mégsem, itt csatoljuk.
        if (db.Entry(transaction).State == EntityState.Detached)
        {
            db.Transactions.Update(transaction);
        }

        db.CategorizationFeedbacks.Add(feedback);

        // Egyetlen SaveChanges: a módosítás és a visszajelzés együtt (egy tranzakcióban) kerül mentésre.
        await db.SaveChangesAsync();
    }
}
