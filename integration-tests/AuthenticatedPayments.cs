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

    public class AuthenticatedPayment : IClassFixture<TestingWebAppFactory<Program>>
    {

        private readonly TestingWebAppFactory<Program> testWebApplicationFactory;
        private readonly HttpClient httpClient;
        public AuthenticatedPayment(TestingWebAppFactory<Program> _testWebApplicationFactory)
        {
            testWebApplicationFactory = _testWebApplicationFactory;
            var options = new WebApplicationFactoryClientOptions();
            options.BaseAddress = new Uri("http://localhost:13502");
            httpClient = testWebApplicationFactory.CreateClient(options);
        }

        [Fact]
        public async Task Payment_Fail_Authenticate_DecodeToken()
        {
            // Arrange
            Merchant merchant = null;
            string cardHolder = "Oscar Valente";
            string pan = "1234-1234-1234-9999";
            string username = "usernameFailAuth1";
            string password = "Password01!";

            using (var scope = testWebApplicationFactory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<PaymentsAPIDbContext>();

                // 1. create customer
                Password.CreatePasswordHash(password, out string passwordHash, out string passwordSalt);
                merchant = new Merchant
                {
                    Username = username,
                    Address = "default address",
                    PasswordSalt = passwordSalt,
                    PasswordHash = passwordHash,
                };
                dbContext.Merchants.Add(merchant);

                dbContext.SaveChanges();
            }

            Dictionary<string, string> signinJsonValues = new Dictionary<string, string>();
            signinJsonValues.Add("username", username);
            signinJsonValues.Add("password", password);
            StringContent signinBody = new StringContent(JsonConvert.SerializeObject(signinJsonValues), UnicodeEncoding.UTF8, "application/json");

            Dictionary<string, string> paymentJsonValues = new Dictionary<string, string>();
            paymentJsonValues.Add("cvv", "323");
            paymentJsonValues.Add("cardHolder", cardHolder);
            paymentJsonValues.Add("pan", pan);
            paymentJsonValues.Add("expiryDate", "31-12-2032");
            paymentJsonValues.Add("amount", "30");
            paymentJsonValues.Add("currencyCode", "eur");

            StringContent paymentBody = new StringContent(JsonConvert.SerializeObject(paymentJsonValues), UnicodeEncoding.UTF8, "application/json");

            // Act

            // not authenticated

            httpClient.DefaultRequestHeaders.Add("Authorization", "invalid-token");
            var paymentResponse = await httpClient.PostAsync($"/api/pay", paymentBody);

            // Assert

            // - HTTP
            Assert.Equal("BadRequest", paymentResponse.StatusCode.ToString());
            var apiError = await paymentResponse.Content.ReadFromJsonAsync<ProblemDetailsError>();
            Assert.Equal("IDX12741: JWT: '[PII of type 'System.String' is hidden. For more details, see https://aka.ms/IdentityModel/PII.]' must have three segments (JWS) or five segments (JWE).", apiError.Detail);
        }

        /// <summary>
        /// Method <c>Payment_Fail_MerchantDeletedAfterAuth</c>
        /// Merchant exists and authenticates, but before paying he gets deleted from DB (requester still holds a valid auth token)
        /// </summary>
        [Fact]
        public async Task Payment_Fail_MerchantDeletedAfterAuth()
        {
            // Arrange
            Merchant merchant = null;

            string username = "usernameFailAuth2";
            string password = "TestPassword1!";

            string cardHolder = "Oscar Valente";
            string pan = "1234-1234-1234-9999";
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

            Dictionary<string, string> signInJsonValues = new Dictionary<string, string>();
            signInJsonValues.Add("username", username);
            signInJsonValues.Add("password", password);

            StringContent signInReqBody = new StringContent(JsonConvert.SerializeObject(signInJsonValues), UnicodeEncoding.UTF8, "application/json");

            Dictionary<string, string> paymentJsonValues = new Dictionary<string, string>();
            paymentJsonValues.Add("cvv", "323");
            paymentJsonValues.Add("cardHolder", cardHolder);
            paymentJsonValues.Add("pan", pan);
            paymentJsonValues.Add("expiryDate", "31-12-2032");
            paymentJsonValues.Add("amount", "30");
            paymentJsonValues.Add("currencyCode", "eur");

            StringContent paymentBody = new StringContent(JsonConvert.SerializeObject(paymentJsonValues), UnicodeEncoding.UTF8, "application/json");

            // Act

            // - HTTP
            var signInResponse = await httpClient.PostAsync("/api/auth/sign-in", signInReqBody);

            // Assert
            // // 1. authenticate
            var tokenBody = await signInResponse.Content.ReadFromJsonAsync<TokenResponse>();

            // 2. merchant gets deleted
            using (var scope = testWebApplicationFactory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<PaymentsAPIDbContext>();


                dbContext.Merchants.Remove(merchant);

                dbContext.SaveChanges();
            }

            // 3. invoke payment
            httpClient.DefaultRequestHeaders.Add("Authorization", tokenBody.Token);
            var paymentResponse = await httpClient.PostAsync($"/api/pay", paymentBody);

            // Assert

            // - HTTP
            Assert.Equal("BadRequest", paymentResponse.StatusCode.ToString());
            var apiError = await paymentResponse.Content.ReadFromJsonAsync<ProblemDetailsError>();
            Assert.Equal("Merchant is not authorized to do this operation", apiError.Detail);
        }
    }
}