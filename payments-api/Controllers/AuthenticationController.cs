using Microsoft.AspNetCore.Mvc;
using PaymentsAPI.Services;
using PaymentsAPI.Errors;
using PaymentsAPI.Entities;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

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
            Entities.Merchant result = null;
            try
            {
                result = signUpService.signUpMerchant(request.username, request.password);
            }
            catch (SignUpException exception)
            {
                return BadRequest(exception.Message);

            }

            if (result != null)
            {

                return NoContent();
            }

            return StatusCode(500, "Unknown error");
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
                return StatusCode(400, $"Sign-in error: {e.Message}");
            }
            catch (Exception e)
            {
                return StatusCode(500, "Unknown error");
            }
        }
    }
}

