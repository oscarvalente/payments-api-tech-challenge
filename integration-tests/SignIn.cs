using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using PaymentsAPI.EfStructures;
using PaymentsAPI.Entities;
using PaymentsAPI.Services;
using PaymentsAPI.Web.Responses;

namespace integration_tests
{
    public class SignInTest : IClassFixture<TestingWebAppFactory<Program>>
    {

        private readonly TestingWebAppFactory<Program> testWebApplicationFactory;
        private readonly HttpClient httpClient;
        public SignInTest(TestingWebAppFactory<Program> _testWebApplicationFactory)
        {
            testWebApplicationFactory = _testWebApplicationFactory;
            var options = new WebApplicationFactoryClientOptions();
            options.BaseAddress = new Uri("http://localhost:13502");
            httpClient = testWebApplicationFactory.CreateClient(options);

        }
        [Fact]
        public async Task SignIn_Success()
        {
            // Arrange
            Merchant merchant = null;
            string username = "testuser1";
            string password = "TestPassword1!";
            using (var scope = testWebApplicationFactory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<PaymentsAPIDbContext>();
                Password.CreatePasswordHash(password, out string passwordHash, out string passwordSalt);
                merchant = new Merchant
                {
                    Username = username,
                    Address = "default address",
                    PasswordSalt = passwordSalt,
                    PasswordHash = passwordHash,
                    IsVerified = true
                };
                dbContext.Merchants.Add(merchant);
                dbContext.SaveChanges();
            }

            Dictionary<string, string> jsonValues = new Dictionary<string, string>();
            jsonValues.Add("username", username);
            jsonValues.Add("password", password);

            StringContent body = new StringContent(JsonConvert.SerializeObject(jsonValues), UnicodeEncoding.UTF8, "application/json");

            // Act

            // - HTTP
            var response = await httpClient.PostAsync("/api/auth/sign-in", body);

            // Assert
            var tokenBody = await response.Content.ReadFromJsonAsync<TokenResponse>();
            Assert.Equal("OK", response.StatusCode.ToString());
            Assert.NotEmpty(JsonConvert.SerializeObject(tokenBody.Token));
        }

        [Fact]
        public async Task SignIn_Fail_Username()
        {
            // Arrange
            Merchant merchant = null;
            string username = "failuser1";
            string password = "TestPassword1!";

            Dictionary<string, string> jsonValues = new Dictionary<string, string>();
            jsonValues.Add("username", username);
            jsonValues.Add("password", password);

            StringContent body = new StringContent(JsonConvert.SerializeObject(jsonValues), UnicodeEncoding.UTF8, "application/json");

            // Act

            // - HTTP
            var response = await httpClient.PostAsync("/api/auth/sign-in", body);

            // Assert
            Assert.Equal("BadRequest", response.StatusCode.ToString());
            var apiError = await response.Content.ReadFromJsonAsync<APIError>();
            Assert.Equal(JsonConvert.SerializeObject(new
            {
                Code = "E-INVALID_CREDENTIALS",
                Message = "Invalid credentials"
            }), JsonConvert.SerializeObject(apiError));
        }

        [Fact]
        public async Task SignIn_Fail_Password()
        {
            // Arrange
            Merchant merchant = null;
            string username = "failuser2";
            string password = "TestPassword1!";
            using (var scope = testWebApplicationFactory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<PaymentsAPIDbContext>();
                Password.CreatePasswordHash(password, out string passwordHash, out string passwordSalt);
                merchant = new Merchant
                {
                    Username = username,
                    Address = "default address",
                    PasswordSalt = passwordSalt,
                    PasswordHash = passwordHash,
                    IsVerified = true
                };
                dbContext.Merchants.Add(merchant);
                dbContext.SaveChanges();
            }

            Dictionary<string, string> jsonValues = new Dictionary<string, string>();
            jsonValues.Add("username", username);
            jsonValues.Add("password", "WrongPassword01!");

            StringContent body = new StringContent(JsonConvert.SerializeObject(jsonValues), UnicodeEncoding.UTF8, "application/json");

            // Act

            // - HTTP
            var response = await httpClient.PostAsync("/api/auth/sign-in", body);

            // Assert
            Assert.Equal("BadRequest", response.StatusCode.ToString());
            var apiError = await response.Content.ReadFromJsonAsync<APIError>();
            Assert.Equal(JsonConvert.SerializeObject(new
            {
                Code = "E-INVALID_CREDENTIALS",
                Message = "Invalid credentials"
            }), JsonConvert.SerializeObject(apiError));
        }

        [Fact]
        public async Task SignIn_Fail_NotVerified()
        {
            // Arrange
            Merchant merchant = null;
            string username = "failuser3";
            string password = "TestPassword1!";
            using (var scope = testWebApplicationFactory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<PaymentsAPIDbContext>();
                Password.CreatePasswordHash(password, out string passwordHash, out string passwordSalt);
                merchant = new Merchant
                {
                    Username = username,
                    Address = "default address",
                    PasswordSalt = passwordSalt,
                    PasswordHash = passwordHash
                };
                dbContext.Merchants.Add(merchant);
                dbContext.SaveChanges();
            }

            Dictionary<string, string> jsonValues = new Dictionary<string, string>();
            jsonValues.Add("username", username);
            jsonValues.Add("password", password);

            StringContent body = new StringContent(JsonConvert.SerializeObject(jsonValues), UnicodeEncoding.UTF8, "application/json");

            // Act

            // - HTTP
            var response = await httpClient.PostAsync("/api/auth/sign-in", body);

            // Assert
            Assert.Equal("BadRequest", response.StatusCode.ToString());
            var apiError = await response.Content.ReadFromJsonAsync<APIError>();
            Assert.Equal(JsonConvert.SerializeObject(new
            {
                Code = "E-MERCHANT_NOT_VERIFIED",
                Message = "Invalid sign-in attempt: Merchant is not verified"
            }), JsonConvert.SerializeObject(apiError));
        }
    }
}
