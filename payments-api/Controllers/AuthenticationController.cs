using Microsoft.AspNetCore.Mvc;
using PaymentsAPI.Services;
using PaymentsAPI.Errors;
using PaymentsAPI.Entities;
using PaymentsAPI.Services.Responses;

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
        private readonly IAPIResponseBuilder apiResponseBuilder;

        public AuthenticationController(IConfiguration _configuration, ISignUp _signUpService, ISignIn _signInService, IToken _tokenService, IAPIResponseBuilder _apiResponseBuilder)
        {
            configuration = _configuration;
            signUpService = _signUpService;
            signInService = _signInService;
            tokenService = _tokenService;
            apiResponseBuilder = _apiResponseBuilder;
        }

        [HttpPost("sign-up")]
        public async Task<ActionResult<JsonContent>> SignUp(AuthenticationPayload request)
        {
            Merchant result = null;
            try
            {
                result = signUpService.signUpMerchant(request.username, request.password);

                if (result == null)
                {
                    return StatusCode(500,
                    Json(apiResponseBuilder.buildInternalError("UNKNOWN", "Unknown error while signing-up merchant")));
                }

                return NoContent();
            }
            catch (SignUpException exception)
            {
                return BadRequest(apiResponseBuilder.buildClientError(exception.code.ToString(), $"Invalid registration: {exception.Message}"));

            }
            catch (Exception exception)
            {
                return StatusCode(500,
                    Json(apiResponseBuilder.buildInternalError("UNKNOWN", "Unknown error while signing-up merchant")));
            }
        }

        [HttpPost("sign-in")]
        public async Task<ActionResult<JsonContent>> SignIn(AuthenticationPayload request)
        {
            try
            {
                if (!signInService.signInMerchant(request.username, request.password))
                {
                    return BadRequest(apiResponseBuilder.buildClientError(SignInExceptionCode.INVALID_CREDENTIALS.ToString(), "Invalid credentials"));
                }

                string token = tokenService.createToken(request.username);

                return Ok(apiResponseBuilder.buildTokenResponse(token));
            }
            catch (SignInException e)
            {
                return StatusCode(400, apiResponseBuilder.buildClientError(e.code.ToString(), $"Invalid sign-in attempt: {e.Message}"));
            }
            catch (Exception e)
            {
                return StatusCode(500, apiResponseBuilder.buildInternalError("UNKNOWN", "Unknown error"));
            }
        }
    }
}

