using System.ComponentModel.DataAnnotations;
using FinanceTracker.Application.Features.Categorization;
using FinanceTracker.Application.Features.Categorization.CreateRule;
using FinanceTracker.Application.Features.Categorization.UpdateRule;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Web.Services;

/// <summary>A szabályűrlap modellje, kliensoldali validációval. A végső ellenőrzést a backend végzi.</summary>
public sealed class RuleFormModel : IValidatableObject
{
    [Required(ErrorMessage = "A kulcsszó megadása kötelező.")]
    [StringLength(CategorizationRule.MaxKeywordLength, ErrorMessage = "A kulcsszó legfeljebb 200 karakter lehet.")]
    public string Keyword { get; set; } = string.Empty;

    public RuleMatchType MatchType { get; set; } = RuleMatchType.Contains;

    public Guid? CategoryId { get; set; }

    [Range(CategorizationRule.MinPriority, CategorizationRule.MaxPriority, ErrorMessage = "A prioritás 0 és 1 000 000 között lehet.")]
    public int Priority { get; set; } = CategorizationRule.DefaultPriority;

    public bool IsActive { get; set; } = true;

    public static RuleFormModel From(CategorizationRuleDto dto) => new()
    {
        Keyword = dto.Keyword,
        MatchType = dto.MatchType,
        CategoryId = dto.CategoryId,
        Priority = dto.Priority,
        IsActive = dto.IsActive
    };

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (CategoryId is null)
        {
            yield return new ValidationResult("A célkategória kiválasztása kötelező.", new[] { nameof(CategoryId) });
        }
    }

    public CreateRuleCommand ToCreateCommand() => new(Keyword.Trim(), MatchType, CategoryId!.Value, Priority, IsActive);

    public UpdateRuleCommand ToUpdateCommand() => new(Keyword.Trim(), MatchType, CategoryId!.Value, Priority, IsActive);
}
