using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using PaymentsAPI.EfStructures;
using PaymentsAPI.Entities;
using PaymentsAPI.Services;
using PaymentsAPI.Services.Responses;

namespace integration_tests
{
    public class ProcessPaymentsTest : IClassFixture<TestingWebAppFactory<Program>>
    {

        private readonly TestingWebAppFactory<Program> testWebApplicationFactory;
        private readonly HttpClient httpClient;
        public ProcessPaymentsTest(TestingWebAppFactory<Program> _testWebApplicationFactory)
        {
            testWebApplicationFactory = _testWebApplicationFactory;
            var options = new WebApplicationFactoryClientOptions();
            options.BaseAddress = new Uri("http://localhost:13502");
            httpClient = testWebApplicationFactory.CreateClient(options);

        }
        [Fact]
        public async Task ProcessPayment_Success_AcquiringBankA()
        {
            // Arrange
            Merchant merchant = null;
            string cardHolder = "Oscar Valente";
            string pan = "1234-1234-1234-0004"; // AcquiringBankA - pan starts with 1234-12 & ends with 0-4 digit
            string username = "usernameSuccess1";
            string password = "TestPassword1!";

            using (var scope = testWebApplicationFactory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<PaymentsAPIDbContext>();

                // 1. create merchant
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

            // authenticate
            var signinResponse = await httpClient.PostAsync($"/api/auth/sign-in", signinBody);

            var tokenReponse = await signinResponse.Content.ReadFromJsonAsync<TokenResponse>();

            httpClient.DefaultRequestHeaders.Add("Authorization", tokenReponse.Token);
            var paymentResponse = await httpClient.PostAsync($"/api/pay", paymentBody);

            // Assert

            // - HTTP
            Assert.Equal("Created", paymentResponse.StatusCode.ToString());
            var result = await paymentResponse.Content.ReadFromJsonAsync<PaymentRefResponse>();

            Assert.True(Guid.TryParse(result.PaymentRef, out var _));

            // - DB
            using (var scope = testWebApplicationFactory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<PaymentsAPIDbContext>();

                Payment payment = dbContext.Payments.SingleOrDefault(p => p.RefUuid == result.PaymentRef);
                Assert.NotNull(payment);
                Assert.Equal("BANKXXXX", payment.AcquiringBankSwift);
                Assert.True(payment.IsAccepted);
                Assert.Equal("OSCAR VALENTE", payment.CardHolder);
            }
        }

        [Fact]
        public async Task ProcessPayment_Fail_AcquiringBankA()
        {
            // Arrange
            Merchant merchant = null;
            string cardHolder = "Oscar Valente";
            string pan = "1234-1234-1234-0008"; // AcquiringBankA - pan starts with 1234-12 but ends with 8
            string username = "usernameFail1";
            string password = "TestPassword1!";

            using (var scope = testWebApplicationFactory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<PaymentsAPIDbContext>();

                // 1. create merchant
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

            // authenticate
            var signinResponse = await httpClient.PostAsync($"/api/auth/sign-in", signinBody);

            var tokenReponse = await signinResponse.Content.ReadFromJsonAsync<TokenResponse>();

            httpClient.DefaultRequestHeaders.Add("Authorization", tokenReponse.Token);
            var paymentResponse = await httpClient.PostAsync($"/api/pay", paymentBody);

            // Assert

            // - HTTP
            Assert.Equal("BadRequest", paymentResponse.StatusCode.ToString());
            var apiError = await paymentResponse.Content.ReadFromJsonAsync<APIErrorPaymentRef>();
            Assert.Equal("E-NOT_AUTHORIZED_BY_BANK", apiError.Code);
            Assert.True(apiError.Message.StartsWith("Payment rejected due to: Rejected by the acquiring bank - reference "));
            Assert.True(Guid.TryParse(apiError.PaymentRef, out var _));

            // - DB
            using (var scope = testWebApplicationFactory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<PaymentsAPIDbContext>();

                Payment payment = dbContext.Payments.SingleOrDefault(p => p.RefUuid == apiError.PaymentRef);
                Assert.NotNull(payment);
                Assert.Equal("BANKXXXX", payment.AcquiringBankSwift);
                Assert.Equal("BANKXXXX", payment.AcquiringBankSwift);
                Assert.False(payment.IsAccepted);
            }
        }

        [Fact]
        public async Task ProcessPayment_Success_AcquiringBankB()
        {
            // Arrange
            Merchant merchant = null;
            string pan = "1234-5634-1234-0000"; // AcquiringBankB - pan starts with 1234-56 & ends with 0 digit
            string username = "usernameSuccess2";
            string password = "TestPassword1!";

            using (var scope = testWebApplicationFactory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<PaymentsAPIDbContext>();

                // 1. create merchant
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

            Dictionary<string, string> signinJsonValues = new Dictionary<string, string>();
            signinJsonValues.Add("username", username);
            signinJsonValues.Add("password", password);
            StringContent signinBody = new StringContent(JsonConvert.SerializeObject(signinJsonValues), UnicodeEncoding.UTF8, "application/json");

            Dictionary<string, string> paymentJsonValues = new Dictionary<string, string>();
            paymentJsonValues.Add("cvv", "323");
            paymentJsonValues.Add("cardHolder", "oscar valente");
            paymentJsonValues.Add("pan", pan);
            paymentJsonValues.Add("expiryDate", "31-12-2032");
            paymentJsonValues.Add("amount", "30");
            paymentJsonValues.Add("currencyCode", "eur");

            StringContent paymentBody = new StringContent(JsonConvert.SerializeObject(paymentJsonValues), UnicodeEncoding.UTF8, "application/json");

            // Act

            // authenticate
            var signinResponse = await httpClient.PostAsync($"/api/auth/sign-in", signinBody);

            var tokenReponse = await signinResponse.Content.ReadFromJsonAsync<TokenResponse>();

            httpClient.DefaultRequestHeaders.Add("Authorization", tokenReponse.Token);
            var paymentResponse = await httpClient.PostAsync($"/api/pay", paymentBody);

            // Assert

            // - HTTP
            Assert.Equal("Created", paymentResponse.StatusCode.ToString());
            var result = await paymentResponse.Content.ReadFromJsonAsync<PaymentRefResponse>();
            Assert.True(Guid.TryParse(result.PaymentRef, out var _));

            // - DB
            using (var scope = testWebApplicationFactory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<PaymentsAPIDbContext>();

                Payment payment = dbContext.Payments.SingleOrDefault(p => p.RefUuid == result.PaymentRef);
                Assert.NotNull(payment);
                Assert.Equal("BANKZZZZ", payment.AcquiringBankSwift);
                Assert.True(payment.IsAccepted);
            }
        }

        [Fact]
        public async Task ProcessPayment_Fail_AcquiringBankB()
        {
            // Arrange
            Merchant merchant = null;
            string pan = "1234-5634-1234-0003"; // AcquiringBankA - pan starts with 1234-56 but does not end with 0
            string username = "usernameFail2";
            string password = "TestPassword1!";

            using (var scope = testWebApplicationFactory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<PaymentsAPIDbContext>();

                // 1. create merchant
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

            Dictionary<string, string> signinJsonValues = new Dictionary<string, string>();
            signinJsonValues.Add("username", username);
            signinJsonValues.Add("password", password);
            StringContent signinBody = new StringContent(JsonConvert.SerializeObject(signinJsonValues), UnicodeEncoding.UTF8, "application/json");

            Dictionary<string, string> paymentJsonValues = new Dictionary<string, string>();
            paymentJsonValues.Add("cvv", "323");
            paymentJsonValues.Add("cardHolder", "oscar valente");
            paymentJsonValues.Add("pan", pan);
            paymentJsonValues.Add("expiryDate", "31-12-2032");
            paymentJsonValues.Add("amount", "30");
            paymentJsonValues.Add("currencyCode", "eur");

            StringContent paymentBody = new StringContent(JsonConvert.SerializeObject(paymentJsonValues), UnicodeEncoding.UTF8, "application/json");

            // Act

            // authenticate
            var signinResponse = await httpClient.PostAsync($"/api/auth/sign-in", signinBody);

            var tokenReponse = await signinResponse.Content.ReadFromJsonAsync<TokenResponse>();

            httpClient.DefaultRequestHeaders.Add("Authorization", tokenReponse.Token);
            var paymentResponse = await httpClient.PostAsync($"/api/pay", paymentBody);

            // Assert

            // - HTTP
            Assert.Equal("BadRequest", paymentResponse.StatusCode.ToString());
            var apiError = await paymentResponse.Content.ReadFromJsonAsync<APIErrorPaymentRef>();
            Assert.Equal("E-NOT_AUTHORIZED_BY_BANK", apiError.Code);
            Assert.True(apiError.Message.StartsWith("Payment rejected due to: Rejected by the acquiring bank - reference "));
            Assert.True(Guid.TryParse(apiError.PaymentRef, out var _));

            // - DB
            using (var scope = testWebApplicationFactory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<PaymentsAPIDbContext>();

                Payment payment = dbContext.Payments.SingleOrDefault(p => p.RefUuid == apiError.PaymentRef);
                Assert.NotNull(payment);
                Assert.Equal("BANKZZZZ", payment.AcquiringBankSwift);
                Assert.False(payment.IsAccepted);
            }
        }

        [Fact]
        public async Task ProcessPayment_Fail_BankNotSupported()
        {
            // Arrange
            Merchant merchant = null;
            string pan = "1234-0034-1234-0003"; // AcquiringBank not found
            string username = "usernameFail3";
            string password = "TestPassword1!";

            using (var scope = testWebApplicationFactory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<PaymentsAPIDbContext>();

                // 1. create merchant
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

            Dictionary<string, string> signinJsonValues = new Dictionary<string, string>();
            signinJsonValues.Add("username", username);
            signinJsonValues.Add("password", password);
            StringContent signinBody = new StringContent(JsonConvert.SerializeObject(signinJsonValues), UnicodeEncoding.UTF8, "application/json");

            Dictionary<string, string> paymentJsonValues = new Dictionary<string, string>();
            paymentJsonValues.Add("cvv", "323");
            paymentJsonValues.Add("cardHolder", "oscar valente");
            paymentJsonValues.Add("pan", pan);
            paymentJsonValues.Add("expiryDate", "31-12-2032");
            paymentJsonValues.Add("amount", "30");
            paymentJsonValues.Add("currencyCode", "eur");

            StringContent paymentBody = new StringContent(JsonConvert.SerializeObject(paymentJsonValues), UnicodeEncoding.UTF8, "application/json");

            // Act

            // authenticate
            var signinResponse = await httpClient.PostAsync($"/api/auth/sign-in", signinBody);

            var tokenReponse = await signinResponse.Content.ReadFromJsonAsync<TokenResponse>();

            httpClient.DefaultRequestHeaders.Add("Authorization", tokenReponse.Token);
            var paymentResponse = await httpClient.PostAsync($"/api/pay", paymentBody);

            // Assert

            // - HTTP
            Assert.Equal("BadRequest", paymentResponse.StatusCode.ToString());
            var apiError = await paymentResponse.Content.ReadFromJsonAsync<APIError>();
            Assert.Equal(JsonConvert.SerializeObject(new
            {
                Code = "E-BANK_NOT_SUPPORTED",
                Message = "Payment rejected due to: Acquiring bank is not supported"
            }), JsonConvert.SerializeObject(apiError));
        }
    }
}
