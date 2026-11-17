using FinanceTracker.Application.Common;
using FinanceTracker.Application.Common.Validation;

namespace FinanceTracker.Application.Features.Profile.UpdateProfile;

internal static class UpdateProfileValidator
{
    public static FieldErrors Validate(UpdateProfileCommand command)
    {
        var errors = new FieldErrors();
        UserFieldRules.ValidateDisplayName(command.DisplayName, errors);
        return errors;
    }
}
