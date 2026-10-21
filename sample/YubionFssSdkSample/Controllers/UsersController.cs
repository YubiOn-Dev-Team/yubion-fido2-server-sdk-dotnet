using YubiOn.Fss.Sdk.Data.Fido;
using YubiOn.Fss.Sdk.Data.User;
using YubiOn.Fss.Sdk.Result;
using YubiOn.Fss.Sdk.Sample.Models;
using YubiOn.Fss.Sdk.Sample.Services;
using Microsoft.AspNetCore.Mvc;

namespace YubiOn.Fss.Sdk.Sample.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private const string SessionKeyFssCookie = "fssSessionData";
    private const string SessionKeyUser = "user";

    private readonly FssSdkProvider _provider;
    private readonly ILogger<UsersController> _logger;

    public UsersController(FssSdkProvider provider, ILogger<UsersController> logger)
    {
        _provider = provider;
        _logger = logger;
    }

    private SessionUserDto? GetCurrentUser() =>
        HttpContext.Session.GetObject<SessionUserDto>(SessionKeyUser);

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var currentUser = GetCurrentUser();
        if (currentUser == null)
        {
            return Unauthorized(new { message = "Unauthorized" });
        }

        try
        {
            var fss = _provider.GetSdk();
            var users = await fss.GetAllUsersAsync();
            return Ok(users);
        }
        catch (FssApiException ex)
        {
            _logger.LogError(ex, "GetUsers failed: {Message}", ex.Message);
            return StatusCode(500, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetUsers failed");
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetUser(string userId)
    {
        var currentUser = GetCurrentUser();
        if (currentUser == null)
        {
            return Unauthorized(new { message = "Unauthorized" });
        }

        try
        {
            var fss = _provider.GetSdk();
            var user = await fss.GetUserAsync(userId);
            return Ok(user);
        }
        catch (FssApiException ex)
        {
            _logger.LogError(ex, "GetUser failed: {Message}", ex.Message);
            return StatusCode(500, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetUser failed");
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpPost("{userId}/credentials/start")]
    public async Task<IActionResult> StartAddCredential(string userId)
    {
        var currentUser = GetCurrentUser();
        if (currentUser == null)
        {
            return Unauthorized(new { message = "Unauthorized" });
        }

        if (currentUser.Id != userId)
        {
            return StatusCode(403, new { message = "Forbidden" });
        }

        try
        {
            var fss = _provider.GetSdk();
            var parameter = new Fido2StartRegisterParameter
            {
                CreationOptionsBase = new Fido2CreationOptionsBase
                {
                    Attestation = AttestationConveyancePreference.Direct,
                    AuthenticatorSelection = new AuthenticatorSelectionCriteria
                    {
                        UserVerification = UserVerificationRequirement.Required,
                        ResidentKey = ResidentKeyRequirement.Required
                    }
                },
                User = new UserDataRegisterParameter
                {
                    UserId = userId
                },
                Options = new Fido2RegisterOptions
                {
                    CreateUserIfNotExists = false,
                    UpdateUserIfExists = false
                }
            };

            var result = await fss.StartRegisterCredentialAsync(parameter);

            if (!string.IsNullOrEmpty(result.SessionCookie))
            {
                HttpContext.Session.SetString(SessionKeyFssCookie, result.SessionCookie);
            }

            return Ok(new { creationOptions = result.Data.CreationOptions });
        }
        catch (FssApiException ex)
        {
            _logger.LogError(ex, "StartAddCredential failed: {Message}", ex.Message);
            return StatusCode(500, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "StartAddCredential failed");
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpPost("{userId}/credentials/finish")]
    public async Task<IActionResult> FinishAddCredential(string userId, [FromBody] Fido2FinishRegisterParameter parameter)
    {
        var currentUser = GetCurrentUser();
        if (currentUser == null)
        {
            return Unauthorized(new { message = "Unauthorized" });
        }

        if (currentUser.Id != userId)
        {
            return StatusCode(403, new { message = "Forbidden" });
        }

        try
        {
            var fssSessionData = HttpContext.Session.GetString(SessionKeyFssCookie);
            if (string.IsNullOrEmpty(fssSessionData))
            {
                return BadRequest(new { message = "Session not found." });
            }

            var fss = _provider.GetSdk();
            var registerResult = await fss.FinishRegisterCredentialAsync(parameter, fssSessionData);

            HttpContext.Session.Remove(SessionKeyFssCookie);
            return Ok(registerResult.User);
        }
        catch (FssApiException ex)
        {
            _logger.LogError(ex, "FinishAddCredential failed: {Message}", ex.Message);
            HttpContext.Session.Remove(SessionKeyFssCookie);
            return StatusCode(500, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "FinishAddCredential failed");
            HttpContext.Session.Remove(SessionKeyFssCookie);
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpDelete("{userId}/credentials/{credentialId}")]
    public async Task<IActionResult> DeleteCredential(string userId, string credentialId)
    {
        var currentUser = GetCurrentUser();
        if (currentUser == null)
        {
            return Unauthorized(new { message = "Unauthorized" });
        }

        if (currentUser.Id != userId)
        {
            return StatusCode(403, new { message = "Forbidden" });
        }

        try
        {
            var fss = _provider.GetSdk();
            await fss.DeleteCredentialAsync(userId, credentialId);
            return NoContent();
        }
        catch (FssApiException ex)
        {
            _logger.LogError(ex, "DeleteCredential failed: {Message}", ex.Message);
            return StatusCode(500, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteCredential failed");
            return StatusCode(500, new { message = ex.Message });
        }
    }
}
