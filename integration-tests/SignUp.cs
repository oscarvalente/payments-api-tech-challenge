using System.ComponentModel;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using PaymentsAPI.EfStructures;
using PaymentsAPI.Entities;
using PaymentsAPI.Services;
using PaymentsAPI.Web.Responses;

namespace integration_tests
{
    public class SignUpTest : IClassFixture<TestingWebAppFactory<Program>>
    {

        private readonly TestingWebAppFactory<Program> testWebApplicationFactory;
        private readonly HttpClient httpClient;
        public SignUpTest(TestingWebAppFactory<Program> _testWebApplicationFactory)
        {
            testWebApplicationFactory = _testWebApplicationFactory;
            var options = new WebApplicationFactoryClientOptions();
            options.BaseAddress = new Uri("http://localhost:13502");
            httpClient = testWebApplicationFactory.CreateClient(options);

        }
        [Fact]
        public async Task SignUp_Success()
        {
            // Arrange

            DateTime datetime = DateTime.Now;
            string username = $"testmerchant1";
            string password = "TestPassword123!";
            Dictionary<string, string> jsonValues = new Dictionary<string, string>();
            jsonValues.Add("username", username);
            jsonValues.Add("password", password);

            StringContent body = new StringContent(JsonConvert.SerializeObject(jsonValues), UnicodeEncoding.UTF8, "application/json");

            // Act

            // - HTTP
            var response = await httpClient.PostAsync("/api/auth/sign-up", body);

            // - DB
            Merchant merchant;
            using (var scope = testWebApplicationFactory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<PaymentsAPIDbContext>();
                merchant = dbContext.Merchants.FirstOrDefault(c => c.Username == username);
            }

            // Assert
            Assert.Equal("Created", response.StatusCode.ToString());
            Assert.Equal("testmerchant1", await response.Content.ReadAsStringAsync());
            Assert.NotNull(merchant);
        }

        [Theory]
        [InlineData("oscar")]
        [InlineData("Oscar ")]
        [InlineData("Oscar 12345")]
        [InlineData("!Oscar!")]
        [InlineData("12345")]
        public async Task SignUp_Fail_Username(string username)
        {
            // Arrange

            DateTime datetime = DateTime.Now;
            string password = "TestPassword123!";
            Dictionary<string, string> jsonValues = new Dictionary<string, string>();
            jsonValues.Add("username", username);
            jsonValues.Add("password", password);

            StringContent body = new StringContent(JsonConvert.SerializeObject(jsonValues), UnicodeEncoding.UTF8, "application/json");

            // Act

            var response = await httpClient.PostAsync("/api/auth/sign-up", body);
            Merchant merchant;
            using (var scope = testWebApplicationFactory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<PaymentsAPIDbContext>();
                merchant = dbContext.Merchants.FirstOrDefault(c => c.Username == username);
            }

            // Assert
            Assert.Equal("BadRequest", response.StatusCode.ToString());
            var apiError = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            Assert.Equal(JsonConvert.SerializeObject(new
            {
                Key = "username",
                Value = new string[1] { "Username must contain only alphanumeric characters with minimum of 8 to maximum of 20 length" }
            }), JsonConvert.SerializeObject(apiError.Errors.ToList().ElementAt(0)));
            Assert.Null(merchant);
        }

        [Theory]
        [InlineData("invalidPassw1")]
        [InlineData("invalidPass word")]
        [InlineData("1234!")]
        [InlineData("pass")]
        [InlineData("Password1")]
        [InlineData("Passwo1!")]
        [InlineData("Passwo1!!!WWWWWWWWWWWWWWW")]
        public async Task SignUp_Fail_Password(string password)
        {
            // Arrange
            DateTime datetime = DateTime.Now;
            string username = $"validUsername22{datetime.Millisecond}";
            Dictionary<string, string> jsonValues = new Dictionary<string, string>();
            jsonValues.Add("username", username);
            jsonValues.Add("password", password);

            StringContent body = new StringContent(JsonConvert.SerializeObject(jsonValues), UnicodeEncoding.UTF8, "application/json");

            // Act

            var response = await httpClient.PostAsync("/api/auth/sign-up", body);
            Merchant merchant;
            using (var scope = testWebApplicationFactory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<PaymentsAPIDbContext>();
                merchant = dbContext.Merchants.FirstOrDefault(c => c.Username == username);
            }

            // Assert
            Assert.Equal("BadRequest", response.StatusCode.ToString());
            var apiError = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            Assert.Equal(JsonConvert.SerializeObject(new
            {
                Key = "password",
                Value = new string[1] { "Password must contain at least 12 characters, alphanumeric and special characters" }
            }), JsonConvert.SerializeObject(apiError.Errors.ToList().ElementAt(0)));
            Assert.Null(merchant);
        }

        [Fact]
        public async Task SignUp_Fail_AlreadyExists()
        {
            // Arrange

            DateTime datetime = DateTime.Now;
            string username = "validUsername23";
            string password = "invalidPassw1!";
            using (var scope = testWebApplicationFactory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<PaymentsAPIDbContext>();
                Password.CreatePasswordHash(password, out string passwordHash, out string passwordSalt);
                Merchant alreadyMerchant = new Merchant
                {
                    Username = username,
                    Address = "default address",
                    PasswordSalt = passwordSalt,
                    PasswordHash = passwordHash,
                };
                dbContext.Merchants.Add(alreadyMerchant);
                dbContext.SaveChanges();
            }

            Dictionary<string, string> jsonValues = new Dictionary<string, string>();
            jsonValues.Add("username", username);
            jsonValues.Add("password", password);

            StringContent body = new StringContent(JsonConvert.SerializeObject(jsonValues), UnicodeEncoding.UTF8, "application/json");

            // Act

            var response = await httpClient.PostAsync("/api/auth/sign-up", body);

            // Assert

            Assert.Equal("BadRequest", response.StatusCode.ToString());
            var apiError = await response.Content.ReadFromJsonAsync<APIError>();
            Assert.Equal(JsonConvert.SerializeObject(new
            {
                Code = "E-USER_ALREADY_EXISTS",
                Message = "Invalid registration: User already exists"
            }), JsonConvert.SerializeObject(apiError));
        }
    }
}
