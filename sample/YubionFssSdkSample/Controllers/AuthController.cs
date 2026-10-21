using System.Text;
using YubiOn.Fss.Sdk.Data.Fido;
using YubiOn.Fss.Sdk.Data.User;
using YubiOn.Fss.Sdk.Result;
using YubiOn.Fss.Sdk.Sample.Models;
using YubiOn.Fss.Sdk.Sample.Services;
using Microsoft.AspNetCore.Mvc;

namespace YubiOn.Fss.Sdk.Sample.Controllers;

[ApiController]
[Route("api")]
public class AuthController : ControllerBase
{
    private const string SessionKeyFssCookie = "fssSessionData";
    private const string SessionKeyUser = "user";

    private readonly FssSdkProvider _provider;
    private readonly ILogger<AuthController> _logger;

    public AuthController(FssSdkProvider provider, ILogger<AuthController> logger)
    {
        _provider = provider;
        _logger = logger;
    }

    private static string GetUserId(string name)
    {
        var bytes = Encoding.UTF8.GetBytes(name);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    // ----------------------------------------------------------------
    // User Registration
    // ----------------------------------------------------------------

    [HttpPost("register/start")]
    public async Task<IActionResult> StartRegistration([FromBody] RegisterStartRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.DisplayName))
            {
                return BadRequest(new { message = "Name and displayName are required." });
            }

            var fss = _provider.GetSdk();
            var userId = GetUserId(request.Name);

            // Check if the user already exists
            try
            {
                var existingUser = await fss.GetUserAsync(userId);
                if (existingUser?.User != null)
                {
                    return BadRequest(new { message = "User already exists." });
                }
            }
            catch (FssApiException)
            {
                // Expected when user does not exist
            }

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
                    UserId = userId,
                    UserName = request.Name,
                    DisplayName = request.DisplayName,
                    Disabled = false
                },
                Options = new Fido2RegisterOptions
                {
                    CreateUserIfNotExists = true,
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
            _logger.LogError(ex, "StartRegistration failed with FssApiException: {Message}", ex.Message);
            return StatusCode(500, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "StartRegistration failed");
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpPost("register/finish")]
    public async Task<IActionResult> FinishRegistration([FromBody] Fido2FinishRegisterParameter parameter)
    {
        try
        {
            var fssSessionData = HttpContext.Session.GetString(SessionKeyFssCookie);
            if (string.IsNullOrEmpty(fssSessionData))
            {
                return BadRequest(new { message = "Registration session not found." });
            }

            var fss = _provider.GetSdk();
            var verifyResult = await fss.VerifyRegisterCredentialAsync(parameter, fssSessionData);
            if (verifyResult.Credential?.UserPresence != true || verifyResult.Credential?.UserVerification != true)
            {
                throw new InvalidOperationException("User presence or verification failed.");
            }

            var finishResult = await fss.FinishRegisterCredentialAsync(parameter, fssSessionData);

            HttpContext.Session.Remove(SessionKeyFssCookie);
            return Ok(finishResult);
        }
        catch (FssApiException ex)
        {
            _logger.LogError(ex, "FinishRegistration failed with FssApiException: {Message}", ex.Message);
            HttpContext.Session.Remove(SessionKeyFssCookie);
            return StatusCode(500, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "FinishRegistration failed");
            HttpContext.Session.Remove(SessionKeyFssCookie);
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // ----------------------------------------------------------------
    // User Authentication (Login)
    // ----------------------------------------------------------------

    [HttpPost("login/start")]
    public async Task<IActionResult> StartLogin()
    {
        try
        {
            var fss = _provider.GetSdk();
            var parameter = new Fido2StartAuthenticateParameter
            {
                RequestOptionsBase = new Fido2RequestOptionsBase
                {
                    UserVerification = UserVerificationRequirement.Required
                }
            };

            var result = await fss.StartAuthenticateAsync(parameter);

            if (!string.IsNullOrEmpty(result.SessionCookie))
            {
                HttpContext.Session.SetString(SessionKeyFssCookie, result.SessionCookie);
            }

            return Ok(new { requestOptions = result.Data.RequestOptions });
        }
        catch (FssApiException ex)
        {
            _logger.LogError(ex, "StartLogin failed with FssApiException: {Message}", ex.Message);
            return StatusCode(500, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "StartLogin failed");
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpPost("login/finish")]
    public async Task<IActionResult> FinishLogin([FromBody] Fido2FinishAuthenticateParameter parameter)
    {
        try
        {
            var fssSessionData = HttpContext.Session.GetString(SessionKeyFssCookie);
            if (string.IsNullOrEmpty(fssSessionData))
            {
                return BadRequest(new { message = "Login session not found." });
            }

            var fss = _provider.GetSdk();
            var result = await fss.FinishAuthenticateAsync(parameter, fssSessionData);

            if (result.User != null)
            {
                HttpContext.Session.SetObject(SessionKeyUser, new SessionUserDto
                {
                    Id = result.User.UserId ?? string.Empty,
                    Name = result.User.UserName ?? string.Empty,
                    DisplayName = result.User.DisplayName ?? string.Empty
                });
            }

            HttpContext.Session.Remove(SessionKeyFssCookie);
            return Ok(result);
        }
        catch (FssApiException ex)
        {
            _logger.LogError(ex, "FinishLogin failed with FssApiException: {Message}", ex.Message);
            HttpContext.Session.Remove(SessionKeyFssCookie);
            return StatusCode(500, new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "FinishLogin failed");
            HttpContext.Session.Remove(SessionKeyFssCookie);
            return StatusCode(500, new { message = ex.Message });
        }
    }

    // ----------------------------------------------------------------
    // Session Management
    // ----------------------------------------------------------------

    [HttpGet("session")]
    public IActionResult CheckSession()
    {
        var user = HttpContext.Session.GetObject<SessionUserDto>(SessionKeyUser);
        if (user != null)
        {
            return Ok(new SessionStateResponse
            {
                LoggedIn = true,
                User = user
            });
        }

        return Ok(new SessionStateResponse
        {
            LoggedIn = false,
            User = null
        });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return Ok(new { message = "Logged out successfully." });
    }
}
