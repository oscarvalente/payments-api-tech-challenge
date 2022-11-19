using Microsoft.AspNetCore.Mvc;
using PaymentsAPI.Services;
using PaymentsAPI.Errors;
using PaymentsAPI.Entities;
namespace PaymentsAPI.Controllers.Authentication
{
    [Route("api/auth")]
    [ApiController]
    public class AuthenticationController : Controller
    {
        private readonly IConfiguration configuration;
        private readonly ISignUp signUpService;
        private readonly ISignIn signInService;
        private readonly IToken tokenService;

        public AuthenticationController(IConfiguration _configuration, ISignUp _signUpService, ISignIn _signInService, IToken _tokenService)
        {
            configuration = _configuration;
            signUpService = _signUpService;
            signInService = _signInService;
            tokenService = _tokenService;
        }

        [HttpPost("sign-up")]
        public async Task<ActionResult<string>> SignUp(AuthenticationPayload request)
        {
            Merchant result = null;
            try
            {
                result = signUpService.signUpMerchant(request.username, request.password);

                if (result == null)
                {
                    return StatusCode(500, "Unknown error while signing-up merchant");
                }

                return NoContent();
            }
            catch (SignUpException exception)
            {
                return BadRequest($"Invalid registration: {exception.Message}");

            }
            catch (Exception exception)
            {
                return StatusCode(500, "Unknown error while signing-up merchant");
            }
        }

        [HttpPost("sign-in")]
        public async Task<ActionResult<string>> SignIn(AuthenticationPayload request)
        {
            try
            {
                if (!signInService.signInMerchant(request.username, request.password))
                {
                    return BadRequest("Invalid credentials");
                }

                string token = tokenService.createToken(request.username);

                return Ok(token);
            }
            catch (SignInException e)
            {
                return StatusCode(400, $"Invalid sign-in attempt: {e.Message}");
            }
            catch (Exception e)
            {
                return StatusCode(500, "Unknown error");
            }
        }
    }
}

