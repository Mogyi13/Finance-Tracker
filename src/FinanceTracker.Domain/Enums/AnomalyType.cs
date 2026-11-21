namespace FinanceTracker.Domain.Enums;

/// <summary>UC-25: az anomália típusa (az adatbázisban tinyint, értékei a DATABASE.MD szerint).</summary>
public enum AnomalyType : byte
{
    /// <summary>Egy tranzakció összege szokatlanul magas a hasonló (azonos kategóriájú és pénznemű) tranzakciókhoz képest.</summary>
    UnusuallyHighAmount = 1,

    /// <summary>Egy kategória havi kiadása jelentősen eltér a szokásos havi szinttől.</summary>
    CategoryDeviation = 2
}
