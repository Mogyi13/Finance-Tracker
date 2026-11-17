using FinanceTracker.API.Infrastructure;
using FinanceTracker.Application.Features.Authentication.Login;
using FinanceTracker.Application.Features.Authentication.Register;
using FinanceTracker.Application.Features.Profile;
using FinanceTracker.Application.Features.Profile.GetProfile;
using FinanceTracker.Application.Features.Profile.UpdateProfile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTracker.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(
    RegisterHandler register,
    LoginHandler login,
    GetProfileHandler getProfile,
    UpdateProfileHandler updateProfile) : ControllerBase
{
    /// <summary>UC-01: új felhasználó regisztrációja.</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType<RegisterResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(RegisterCommand command)
    {
        var result = await register.HandleAsync(command);
        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, result.Value)
            : this.ToProblem(result.Error!);
    }

    /// <summary>UC-02: bejelentkezés, JWT kiállítása.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status423Locked)]
    public async Task<IActionResult> Login(LoginCommand command)
    {
        var result = await login.HandleAsync(command);
        return result.IsSuccess ? Ok(result.Value) : this.ToProblem(result.Error!);
    }

    /// <summary>UC-04: a bejelentkezett felhasználó profilja.</summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType<UserProfileDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Me()
    {
        var result = await getProfile.HandleAsync();
        return result.IsSuccess ? Ok(result.Value) : this.ToProblem(result.Error!);
    }

    /// <summary>UC-04: profil módosítása (megjelenítendő név).</summary>
    [HttpPut("profile")]
    [Authorize]
    [ProducesResponseType<UserProfileDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateProfile(UpdateProfileCommand command)
    {
        var result = await updateProfile.HandleAsync(command);
        return result.IsSuccess ? Ok(result.Value) : this.ToProblem(result.Error!);
    }
}
