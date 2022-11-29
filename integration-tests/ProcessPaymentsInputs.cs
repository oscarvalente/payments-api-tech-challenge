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
    public class ProcessPaymentsInputsTest : IClassFixture<TestingWebAppFactory<Program>>
    {

        private readonly TestingWebAppFactory<Program> testWebApplicationFactory;
        private readonly HttpClient httpClient;
        public ProcessPaymentsInputsTest(TestingWebAppFactory<Program> _testWebApplicationFactory)
        {
            testWebApplicationFactory = _testWebApplicationFactory;
            var options = new WebApplicationFactoryClientOptions();
            options.BaseAddress = new Uri("http://localhost:13502");
            httpClient = testWebApplicationFactory.CreateClient(options);

        }

        [Fact]
        public async Task Payment_Fail_Input_CardHolder()
        {
            // Arrange
            Merchant merchant = null;
            string username = "usernameInput1";
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
            paymentJsonValues.Add("cardHolder", "oscar "); // invalid card holder - ERROR
            paymentJsonValues.Add("pan", "1234-1234-1234-1234");
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
                Code = "E-INVALID_FORMAT_CARD_HOLDER",
                Message = "Invalid card holder format"
            }), JsonConvert.SerializeObject(apiError));
        }

        [Fact]
        public async Task Payment_Fail_Input_PAN()
        {
            // Arrange
            Merchant merchant = null;
            string username = "usernameInput2";
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
            paymentJsonValues.Add("cvv", "Aab");
            paymentJsonValues.Add("cardHolder", "oscar valente");
            paymentJsonValues.Add("pan", "1234-1234-ZZZZ-ZZZ"); // invalid PAN - ERROR
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
                Code = "E-INVALID_FORMAT_CARD_NUMBER",
                Message = "Invalid card number format"
            }), JsonConvert.SerializeObject(apiError));
        }

        [Fact]
        public async Task Payment_Fail_Input_InvalidExpiryDate()
        {
            // Arrange
            Merchant merchant = null;
            string pan = "1234-1234-1234-9999";
            string username = "usernameInput3";
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
            paymentJsonValues.Add("expiryDate", "invalid-date"); // invalid expiry date - ERROR
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
                Code = "E-INVALID_FORMAT_EXPIRY_DATE",
                Message = "Invalid expiry date"
            }), JsonConvert.SerializeObject(apiError));
        }

        [Fact]
        public async Task Payment_Fail_Input_CardExpired()
        {
            // Arrange
            Merchant merchant = null;
            string pan = "1234-1234-1234-9999";
            string username = "usernameInput4";
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
            paymentJsonValues.Add("expiryDate", "31-12-2020"); // expired date - ERROR
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
                Code = "E-INVALID_CARD_EXPIRED",
                Message = "Card has expired"
            }), JsonConvert.SerializeObject(apiError));
        }

        [Fact]
        public async Task Payment_Fail_Input_CVV()
        {

            // Arrange
            Merchant merchant = null;
            string pan = "1234-1234-1234-9999";
            string username = "usernameInput5";
            string password = "TestPassword1!";

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
            paymentJsonValues.Add("cvv", "invalid-cvv"); // invalid CVV - ERROR
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
                Code = "E-INVALID_FORMAT_CVV",
                Message = "Invalid CVV format"
            }), JsonConvert.SerializeObject(apiError));
        }

        [Fact]
        public async Task Payment_Fail_Input_InvalidAmount_Multiple() // format, range, ...
        {
            // Arrange
            Payment payment = null;
            Merchant merchant = null;
            string pan = "1234-1234-1234-9999";
            string username = "usernameInput6";
            string password = "TestPassword1!";

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
            paymentJsonValues.Add("currencyCode", "eur");

            StringContent paymentBody = new StringContent(JsonConvert.SerializeObject(paymentJsonValues), UnicodeEncoding.UTF8, "application/json");

            // Act

            // authenticate
            var signinResponse = await httpClient.PostAsync($"/api/auth/sign-in", signinBody);

            var tokenReponse = await signinResponse.Content.ReadFromJsonAsync<TokenResponse>();

            httpClient.DefaultRequestHeaders.Add("Authorization", tokenReponse.Token);

            paymentJsonValues.Add("amount", "AAAA"); // invalid amount - ERROR
            var paymentResponseFormat = await httpClient.PostAsync($"/api/pay", paymentBody);

            paymentJsonValues.Remove("amount");
            paymentJsonValues.Add("amount", "100000");
            var paymentResponseMaxRange = await httpClient.PostAsync($"/api/pay", paymentBody);

            paymentJsonValues.Remove("amount");
            paymentJsonValues.Add("amount", "0");
            var paymentResponseMinRange = await httpClient.PostAsync($"/api/pay", paymentBody);

            // Assert

            // - HTTP
            Assert.Equal("BadRequest", paymentResponseFormat.StatusCode.ToString());
            var apiError = await paymentResponseFormat.Content.ReadFromJsonAsync<APIError>();
            Assert.Equal(JsonConvert.SerializeObject(new
            {
                Code = "E-INVALID_FORMAT_AMOUNT",
                Message = "Invalid amount. Only payments up to 500 are allowed"
            }), JsonConvert.SerializeObject(apiError));

            Assert.Equal("BadRequest", paymentResponseMaxRange.StatusCode.ToString());
            var apiError2 = await paymentResponseMaxRange.Content.ReadFromJsonAsync<APIError>();
            Assert.Equal(JsonConvert.SerializeObject(new
            {
                Code = "E-INVALID_FORMAT_AMOUNT",
                Message = "Invalid amount. Only payments up to 500 are allowed"
            }), JsonConvert.SerializeObject(apiError2));

            Assert.Equal("BadRequest", paymentResponseMinRange.StatusCode.ToString());
            var apiError3 = await paymentResponseMinRange.Content.ReadFromJsonAsync<APIError>();
            Assert.Equal(JsonConvert.SerializeObject(new
            {
                Code = "E-INVALID_FORMAT_AMOUNT",
                Message = "Invalid amount. Only payments up to 500 are allowed"
            }), JsonConvert.SerializeObject(apiError3));
        }

        [Fact]
        public async Task Payment_Fail_Input_InvalidCurrencyCodeFormat()
        {
            // Arrange
            Merchant merchant = null;
            string username = "usernameInput7";
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
            paymentJsonValues.Add("pan", "1234-1234-1234-1234");
            paymentJsonValues.Add("expiryDate", "31-12-2032");
            paymentJsonValues.Add("amount", "30");
            paymentJsonValues.Add("currencyCode", "eu"); // invalid currency code - ERROR

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
                Code = "E-INVALID_FORMAT_CURRENCY",
                Message = "Invalid currency code format"
            }), JsonConvert.SerializeObject(apiError));
        }

        [Fact]
        public async Task Payment_Fail_Input_CurrencyCodeNotSupported()
        {
            // Arrange
            Merchant merchant = null;
            string username = "usernameInput8";
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
            paymentJsonValues.Add("pan", "1234-1234-1234-1234");
            paymentJsonValues.Add("expiryDate", "31-12-2032");
            paymentJsonValues.Add("amount", "30");
            paymentJsonValues.Add("currencyCode", "JPY"); // currency code not supported - ERROR

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
                Code = "E-CURRENCY_NOT_SUPPORTED",
                Message = "Currency code is not supported"
            }), JsonConvert.SerializeObject(apiError));
        }
    }
}
