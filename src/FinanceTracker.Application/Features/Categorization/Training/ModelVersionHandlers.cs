using FinanceTracker.Application.Common;
using FinanceTracker.Application.Features.Categorization.Ml;

namespace FinanceTracker.Application.Features.Categorization.Training;

/// <summary>UC-20: a modellverziók és kiértékelési eredményeik áttekintése (dokumentálás).</summary>
public sealed class GetModelVersionsHandler(IMlModelRepository models)
{
    public async Task<Result<IReadOnlyList<ModelVersionDto>>> HandleAsync()
    {
        var all = await models.GetAllAsync();
        return Result<IReadOnlyList<ModelVersionDto>>.Ok(all.Select(ModelVersionDto.From).ToList());
    }
}

public sealed class GetModelVersionHandler(IMlModelRepository models)
{
    public async Task<Result<ModelVersionDetailsDto>> HandleAsync(Guid id)
    {
        var version = await models.GetByIdAsync(id);
        return version is null
            ? Error.NotFound("A modellverzió nem található.")
            : Result<ModelVersionDetailsDto>.Ok(ModelVersionDetailsDto.From(version));
    }
}

/// <summary>
/// UC-20: modellverzió aktiválása. Csak az aktiválási feltételeknek megfelelő, létező modellfájllal rendelkező
/// verzió aktiválható; az előző aktív verzió ugyanabban a lépésben inaktívvá válik (egyszerre egy aktív modell van).
/// A visszajelzések rögzítése sosem aktivál automatikusan új modellt.
/// </summary>
public sealed class ActivateModelHandler(IMlModelRepository models, IMlModelStore store, TimeProvider timeProvider)
{
    public async Task<Result<ModelVersionDto>> HandleAsync(Guid id)
    {
        var version = await models.GetByIdAsync(id);
        if (version is null)
        {
            return Error.NotFound("A modellverzió nem található.");
        }

        if (version.IsActive)
        {
            return Result<ModelVersionDto>.Ok(ModelVersionDto.From(version));
        }

        if (!version.MeetsCriteria)
        {
            return Error.Conflict("A modellverzió nem teljesíti az aktiválási feltételeket, ezért nem aktiválható. A részleteket a kiértékelési jelentés tartalmazza.");
        }

        if (!store.Exists(version.ModelPath))
        {
            return Error.Conflict("A modellverzió fájlja hiányzik a modelltárból, ezért nem aktiválható.");
        }

        await models.ActivateAsync(version, timeProvider.GetUtcNow().UtcDateTime);
        return Result<ModelVersionDto>.Ok(ModelVersionDto.From(version));
    }
}

/// <summary>UC-20: az aktív modell kivezetése. Ezután csak a szabályok kategorizálnak automatikusan.</summary>
public sealed class DeactivateModelHandler(IMlModelRepository models)
{
    public async Task<Result<ModelVersionDto>> HandleAsync(Guid id)
    {
        var version = await models.GetByIdAsync(id);
        if (version is null)
        {
            return Error.NotFound("A modellverzió nem található.");
        }

        if (version.IsActive)
        {
            await models.DeactivateAsync(version);
        }

        return Result<ModelVersionDto>.Ok(ModelVersionDto.From(version));
    }
}
