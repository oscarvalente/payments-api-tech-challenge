using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using PaymentsAPI.EfStructures;
using PaymentsAPI.Entities;
using PaymentsAPI.Services;

namespace integration_tests
{
    public class GetPaymentTest : IClassFixture<TestingWebAppFactory<Program>>
    {

        private readonly TestingWebAppFactory<Program> testWebApplicationFactory;
        private readonly HttpClient httpClient;
        public GetPaymentTest(TestingWebAppFactory<Program> _testWebApplicationFactory)
        {
            testWebApplicationFactory = _testWebApplicationFactory;
            var options = new WebApplicationFactoryClientOptions();
            options.BaseAddress = new Uri("http://localhost:13502");
            httpClient = testWebApplicationFactory.CreateClient(options);

        }

        [Fact]
        public async Task GetPayment_Accepted_Success()
        {
            // Arrange
            Merchant merchant = null;
            Payment payment = null;
            string username = "usernameGetPaym1";
            string password = "Password01!";
            string paymentRef = Guid.NewGuid().ToString();

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

                // 2. create payment
                payment = new Payment
                {
                    RefUuid = paymentRef,
                    Amount = 30.50M,
                    CurrencyCode = "EUR",
                    CardHolder = "Oscar Valente",
                    Pan = "1234-1234-1234-0004",
                    ExpiryDate = new DateOnly(2032, 12, 31),
                    AcquiringBankSwift = "BANKXXX",
                    IsAccepted = true,
                    MerchantId = merchant.Id
                };

                dbContext.Payments.Add(payment);


                dbContext.SaveChanges();
            }

            Dictionary<string, string> signinJsonValues = new Dictionary<string, string>();
            signinJsonValues.Add("username", username);
            signinJsonValues.Add("password", password);
            StringContent signinBody = new StringContent(JsonConvert.SerializeObject(signinJsonValues), UnicodeEncoding.UTF8, "application/json");

            // Act

            // authenticate
            var signinResponse = await httpClient.PostAsync($"/api/auth/sign-in", signinBody);

            string token = await signinResponse.Content.ReadAsStringAsync();

            httpClient.DefaultRequestHeaders.Add("Authorization", token);
            var paymentResponse = await httpClient.GetAsync($"/api/payment/{paymentRef}");

            // Assert

            // - HTTP
            Assert.Equal("OK", paymentResponse.StatusCode.ToString());
            var paymentVM = await paymentResponse.Content.ReadFromJsonAsync<PaymentViewModel>();
            var expectedPaymentVM = new PaymentViewModel(payment);
            Assert.Equal(JsonConvert.SerializeObject(expectedPaymentVM), JsonConvert.SerializeObject(paymentVM));
        }

        [Fact]
        public async Task GetPayment_Rejected_Success()
        {
            // Arrange
            Merchant merchant = null;
            Payment payment = null;
            string username = "usernameGetPaym2";
            string password = "Password01!";
            string paymentRef = Guid.NewGuid().ToString();

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

                // 2. create payment
                payment = new Payment
                {
                    RefUuid = paymentRef,
                    Amount = 30.50M,
                    CurrencyCode = "EUR",
                    Pan = "1234-1234-1234-0004",
                    CardHolder = "Oscar Valente",
                    ExpiryDate = new DateOnly(2032, 12, 31),
                    AcquiringBankSwift = "BANKXXX",
                    MerchantId = merchant.Id,
                    IsAccepted = false
                };

                dbContext.Payments.Add(payment);


                dbContext.SaveChanges();
            }

            Dictionary<string, string> signinJsonValues = new Dictionary<string, string>();
            signinJsonValues.Add("username", username);
            signinJsonValues.Add("password", password);
            StringContent signinBody = new StringContent(JsonConvert.SerializeObject(signinJsonValues), UnicodeEncoding.UTF8, "application/json");

            // Act

            // authenticate
            var signinResponse = await httpClient.PostAsync($"/api/auth/sign-in", signinBody);

            string token = await signinResponse.Content.ReadAsStringAsync();

            httpClient.DefaultRequestHeaders.Add("Authorization", token);
            var paymentResponse = await httpClient.GetAsync($"/api/payment/{paymentRef}");

            // Assert

            // - HTTP
            Assert.Equal("OK", paymentResponse.StatusCode.ToString());
            var paymentVM = await paymentResponse.Content.ReadFromJsonAsync<PaymentViewModel>();
            var expectedPaymentVM = new PaymentViewModel(payment);
            Assert.Equal(JsonConvert.SerializeObject(expectedPaymentVM), JsonConvert.SerializeObject(paymentVM));
        }

        /// <summary>
        /// Method <c>GetPayment_Fail_UnauthorizedMerchant</c>
        /// Merchant tries to read payment that does not belong to him
        /// </summary>
        [Fact]
        public async Task GetPayment_Fail_UnauthorizedMerchant()
        {
            // Arrange
            Merchant merchantA = null;
            Merchant merchantB = null;
            Payment payment = null;
            string usernameA = "usernameGetPaymF1";
            string usernameB = "usernameGetPaymF2";
            string password = "Password01!";
            string paymentRef = Guid.NewGuid().ToString();

            using (var scope = testWebApplicationFactory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<PaymentsAPIDbContext>();

                // 1.1 create merchant A
                Password.CreatePasswordHash(password, out string passwordHash, out string passwordSalt);
                merchantA = new Merchant
                {
                    Username = usernameA,
                    Address = "default address A",
                    PasswordSalt = passwordSalt,
                    PasswordHash = passwordHash,
                    IsVerified = true
                };
                dbContext.Merchants.Add(merchantA);

                // 1.1 create merchant B
                Password.CreatePasswordHash(password, out string passwordHashB, out string passwordSaltB);
                merchantB = new Merchant
                {
                    Username = usernameB,
                    Address = "default address B",
                    PasswordSalt = passwordSaltB,
                    PasswordHash = passwordHashB,
                    IsVerified = true
                };
                dbContext.Merchants.Add(merchantB);

                // 2. create payment and associate it to merchant A
                payment = new Payment
                {
                    RefUuid = paymentRef,
                    Amount = 30.50M,
                    CurrencyCode = "EUR",
                    Pan = "1234-1234-1234-0004",
                    CardHolder = "Oscar Valente",
                    ExpiryDate = new DateOnly(2032, 12, 31),
                    AcquiringBankSwift = "BANKXXX",
                    MerchantId = merchantA.Id,
                    IsAccepted = false
                };

                dbContext.Payments.Add(payment);

                dbContext.SaveChanges();
            }

            Dictionary<string, string> signinJsonValues = new Dictionary<string, string>();
            signinJsonValues.Add("username", usernameB); // sign-in with merchant B
            signinJsonValues.Add("password", password);
            StringContent signinBody = new StringContent(JsonConvert.SerializeObject(signinJsonValues), UnicodeEncoding.UTF8, "application/json");

            // Act

            // authenticate
            var signinResponse = await httpClient.PostAsync($"/api/auth/sign-in", signinBody);

            string token = await signinResponse.Content.ReadAsStringAsync();

            httpClient.DefaultRequestHeaders.Add("Authorization", token); // use merchant B token
            var paymentResponse = await httpClient.GetAsync($"/api/payment/{paymentRef}"); // read payment belonging to merchant A

            // Assert

            // - HTTP
            Assert.Equal("NotFound", paymentResponse.StatusCode.ToString());
            Assert.Equal("Payment not found", await paymentResponse.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task GetPayment_Fail_NotFound()
        {
            // Arrange
            Merchant merchant = null;
            string username = "usernameGetPaymF3";
            string password = "Password01!";
            string paymentRef = Guid.NewGuid().ToString();

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

            // Act

            // authenticate
            var signinResponse = await httpClient.PostAsync($"/api/auth/sign-in", signinBody);

            string token = await signinResponse.Content.ReadAsStringAsync();

            httpClient.DefaultRequestHeaders.Add("Authorization", token);
            var paymentResponse = await httpClient.GetAsync($"/api/payment/{Guid.NewGuid().ToString()}");

            // Assert

            // - HTTP
            Assert.Equal("NotFound", paymentResponse.StatusCode.ToString());
            Assert.Equal("Payment not found", await paymentResponse.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task GetPayment_Fail_InvalidPaymentRefFormat()
        {
            // Arrange
            Merchant merchant = null;
            string username = "usernameGetPaymF3b";
            string password = "Password01!";
            string paymentRef = Guid.NewGuid().ToString();

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

            // Act

            // authenticate
            var signinResponse = await httpClient.PostAsync($"/api/auth/sign-in", signinBody);

            string token = await signinResponse.Content.ReadAsStringAsync();

            httpClient.DefaultRequestHeaders.Add("Authorization", token);
            var paymentResponse = await httpClient.GetAsync($"/api/payment/zzzzzz-ZZZ");

            // Assert

            // - HTTP
            Assert.Equal("BadRequest", paymentResponse.StatusCode.ToString());
            Assert.Equal("Invalid payment reference format", await paymentResponse.Content.ReadAsStringAsync());
        }

        /// <summary>
        /// Method <c>GetPayment_Fail_MerchantDeletedAfterAuth</c>
        /// Merchant exists and authenticates, but before getting the accessible payment he gets deleted from DB (requester still holds a valid auth token)
        /// </summary>
        [Fact]
        public async Task GetPayment_Fail_MerchantDeletedAfterAuth()
        {
            // Arrange
            Payment payment = null;
            Merchant merchant = null;
            string cardHolder = "Oscar Valente";
            string pan = "1234-1234-1234-9999";
            string username = "usernameGetPaymF4";
            string password = "Password01!";
            string paymentRef = Guid.NewGuid().ToString();

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

                // 2. create payment
                payment = new Payment
                {
                    RefUuid = paymentRef,
                    Amount = 30.50M,
                    CurrencyCode = "EUR",
                    CardHolder = "Oscar Valente",
                    Pan = "1234-1234-1234-0004",
                    ExpiryDate = new DateOnly(2032, 12, 31),
                    AcquiringBankSwift = "BANKXXX",
                    IsAccepted = true,
                    MerchantId = merchant.Id
                };

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

            // 1. authenticate
            var signinResponse = await httpClient.PostAsync($"/api/auth/sign-in", signinBody);

            string token = await signinResponse.Content.ReadAsStringAsync();

            // 2. merchant gets deleted
            using (var scope = testWebApplicationFactory.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<PaymentsAPIDbContext>();


                dbContext.Merchants.Remove(merchant);

                dbContext.SaveChanges();
            }

            // 3. invoke get payment
            httpClient.DefaultRequestHeaders.Add("Authorization", token);
            var paymentResponse = await httpClient.GetAsync($"/api/payment/{Guid.NewGuid().ToString()}");

            // Assert

            // - HTTP
            Assert.Equal("Forbidden", paymentResponse.StatusCode.ToString());
            Assert.Equal("Merchant is not authorized to get payment", await paymentResponse.Content.ReadAsStringAsync());
        }
    }
}