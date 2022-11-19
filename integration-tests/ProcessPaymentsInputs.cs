using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using PaymentsAPI.EfStructures;
using PaymentsAPI.Entities;
using PaymentsAPI.Services;

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
            string password = "Password01!";

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

            string token = await signinResponse.Content.ReadAsStringAsync();

            httpClient.DefaultRequestHeaders.Add("Authorization", token);
            var paymentResponse = await httpClient.PostAsync($"/api/pay", paymentBody);

            // Assert

            // - HTTP
            Assert.Equal("BadRequest", paymentResponse.StatusCode.ToString());
            Assert.Equal("Invalid card holder format", await paymentResponse.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Payment_Fail_Input_PAN()
        {
            // Arrange
            Merchant merchant = null;
            string username = "usernameInput2";
            string password = "Password01!";

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

            string token = await signinResponse.Content.ReadAsStringAsync();

            httpClient.DefaultRequestHeaders.Add("Authorization", token);
            var paymentResponse = await httpClient.PostAsync($"/api/pay", paymentBody);

            // Assert

            // - HTTP
            Assert.Equal("BadRequest", paymentResponse.StatusCode.ToString());
            Assert.Equal("Invalid card number format", await paymentResponse.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Payment_Fail_Input_InvalidExpiryDate()
        {
            // Arrange
            Merchant merchant = null;
            string pan = "1234-1234-1234-9999";
            string username = "usernameInput3";
            string password = "Password01!";

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

            string token = await signinResponse.Content.ReadAsStringAsync();

            httpClient.DefaultRequestHeaders.Add("Authorization", token);
            var paymentResponse = await httpClient.PostAsync($"/api/pay", paymentBody);

            // Assert

            // - HTTP
            Assert.Equal("BadRequest", paymentResponse.StatusCode.ToString());
            Assert.Equal("Invalid expiry date", await paymentResponse.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Payment_Fail_Input_CardExpired()
        {
            // Arrange
            Merchant merchant = null;
            string pan = "1234-1234-1234-9999";
            string username = "usernameInput4";
            string password = "Password01!";

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

            string token = await signinResponse.Content.ReadAsStringAsync();

            httpClient.DefaultRequestHeaders.Add("Authorization", token);
            var paymentResponse = await httpClient.PostAsync($"/api/pay", paymentBody);

            // Assert

            // - HTTP
            Assert.Equal("BadRequest", paymentResponse.StatusCode.ToString());
            Assert.Equal("Card has expired", await paymentResponse.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Payment_Fail_Input_CVV()
        {

            // Arrange
            Merchant merchant = null;
            string pan = "1234-1234-1234-9999";
            string username = "usernameInput5";
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

            string token = await signinResponse.Content.ReadAsStringAsync();

            httpClient.DefaultRequestHeaders.Add("Authorization", token);
            var paymentResponse = await httpClient.PostAsync($"/api/pay", paymentBody);

            // Assert

            // - HTTP
            Assert.Equal("BadRequest", paymentResponse.StatusCode.ToString());
            Assert.Equal("Invalid CVV format", await paymentResponse.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Payment_Fail_Input_InvalidAmount_Multiple() // format, range, ...
        {
            // Arrange
            Payment payment = null;
            Merchant merchant = null;
            string pan = "1234-1234-1234-9999";
            string username = "usernameInput6";
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

            string token = await signinResponse.Content.ReadAsStringAsync();

            httpClient.DefaultRequestHeaders.Add("Authorization", token);

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
            Assert.Equal("Invalid amount. Only payments up to 500 are allowed", await paymentResponseFormat.Content.ReadAsStringAsync());

            Assert.Equal("BadRequest", paymentResponseMaxRange.StatusCode.ToString());
            Assert.Equal("Invalid amount. Only payments up to 500 are allowed", await paymentResponseMaxRange.Content.ReadAsStringAsync());

            Assert.Equal("BadRequest", paymentResponseMinRange.StatusCode.ToString());
            Assert.Equal("Invalid amount. Only payments up to 500 are allowed", await paymentResponseMinRange.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Payment_Fail_Input_InvalidCurrencyCodeFormat()
        {
            // Arrange
            Merchant merchant = null;
            string username = "usernameInput7";
            string password = "Password01!";

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

            string token = await signinResponse.Content.ReadAsStringAsync();

            httpClient.DefaultRequestHeaders.Add("Authorization", token);
            var paymentResponse = await httpClient.PostAsync($"/api/pay", paymentBody);

            // Assert

            // - HTTP
            Assert.Equal("BadRequest", paymentResponse.StatusCode.ToString());
            Assert.Equal("Invalid currency code format", await paymentResponse.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Payment_Fail_Input_CurrencyCodeNotSupported()
        {
            // Arrange
            Merchant merchant = null;
            string username = "usernameInput8";
            string password = "Password01!";

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

            string token = await signinResponse.Content.ReadAsStringAsync();

            httpClient.DefaultRequestHeaders.Add("Authorization", token);
            var paymentResponse = await httpClient.PostAsync($"/api/pay", paymentBody);

            // Assert

            // - HTTP
            Assert.Equal("BadRequest", paymentResponse.StatusCode.ToString());
            Assert.Equal("Currency code is not supported", await paymentResponse.Content.ReadAsStringAsync());
        }
    }
}
