using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PaymentsAPI.Controllers.Payments;
using PaymentsAPI.Entities;
using PaymentsAPI.Errors;
using PaymentsAPI.Services;

namespace unit_tests
{
    public class PaymentsControllerTest
    {
        // these tests roughly overlapping integration, but they test more input pattern possibilities
        [Fact]
        public async Task Payment_Fail_InvalidToken()
        {
            // Arrange
            string username = "username";
            string pan = "1234-1234-1234-1234";
            var paymentRef = "1234";
            var cardHolder = "Oscar Valente";
            var expiryDate = "01-01-2023";
            var cvv = "323";
            var amount = 55;
            var currencyCode = "EUR";
            Merchant merchant = new Merchant
            {
                Id = 1,
                Username = "merchantA",
                Address = "address",
                PasswordHash = "default",
                PasswordSalt = "default",
                IsVerified = true,
                CreatedAt = DateTime.Now
            };

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Authorization"] = "token";

            var payload = new PaymentsPayload
            {
                cardHolder = cardHolder,
                pan = pan,
                expiryDate = expiryDate,
                cvv = cvv,
                amount = amount,
                currencyCode = currencyCode
            };

            var mockToken = new Mock<IToken>();
            mockToken.Setup(t => t.verifyToken("token"))
            .Returns((string)null);
            var mockCurrencyValidator = new Mock<ICurrencyValidator>();
            mockCurrencyValidator.Setup(cv => cv.isCurrencySupported("eur"))
            .Returns(false);
            var mockMerchantService = new Mock<IMerchant>();
            mockMerchantService.Setup(ms => ms.getMerchantByUsername(username))
            .Returns(merchant);
            var mockPaymentService = new Mock<IPayment>();
            mockPaymentService.Setup(ps => ps.pay(merchant, paymentRef, payload.cardHolder, "1234123412341234", DateOnly.Parse(payload.expiryDate), payload.cvv, payload.amount, payload.currencyCode))
            .Returns("");
            var controller = new PaymentController(mockToken.Object, mockCurrencyValidator.Object, mockMerchantService.Object, mockPaymentService.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = httpContext
                }
            };

            // Act
            var result = (await controller.Pay(payload)).Result as ObjectResult;

            // Assert
            mockToken.Verify(ts => ts.verifyToken("token"), Times.Once);
            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Failed to authenticate", result.Value);
        }

        [Fact]
        public async Task Payment_Fail_TokenDecode()
        {
            // Arrange
            string username = "username";
            string pan = "1234-1234-1234-1234";
            var paymentRef = "1234";
            var cardHolder = "Oscar Valente";
            var expiryDate = "01-01-2023";
            var cvv = "323";
            var amount = 55;
            var currencyCode = "EUR";
            Merchant merchant = new Merchant
            {
                Id = 1,
                Username = "merchantA",
                Address = "address",
                PasswordHash = "default",
                PasswordSalt = "default",
                IsVerified = true,
                CreatedAt = DateTime.Now
            };

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Authorization"] = "token";

            var payload = new PaymentsPayload
            {
                cardHolder = cardHolder,
                pan = pan,
                expiryDate = expiryDate,
                cvv = cvv,
                amount = amount,
                currencyCode = currencyCode
            };

            var mockToken = new Mock<IToken>();
            mockToken.Setup(t => t.verifyToken("token"))
            .Throws(new Exception());
            var mockCurrencyValidator = new Mock<ICurrencyValidator>();
            mockCurrencyValidator.Setup(cv => cv.isCurrencySupported("eur"))
            .Returns(false);
            var mockMerchantService = new Mock<IMerchant>();
            mockMerchantService.Setup(ms => ms.getMerchantByUsername(username))
            .Returns(merchant);
            var mockPaymentService = new Mock<IPayment>();
            mockPaymentService.Setup(ps => ps.pay(merchant, paymentRef, payload.cardHolder, "1234123412341234", DateOnly.Parse(payload.expiryDate), payload.cvv, payload.amount, payload.currencyCode))
            .Returns("");
            var controller = new PaymentController(mockToken.Object, mockCurrencyValidator.Object, mockMerchantService.Object, mockPaymentService.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = httpContext
                }
            };

            // Act
            var result = (await controller.Pay(payload)).Result as ObjectResult;

            // Assert
            mockToken.Verify(ts => ts.verifyToken("token"), Times.Once);
            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Failed to authenticate", result.Value);
        }

        [Fact]
        public async Task Payment_Fail_UnauthorizedMerchant()
        {
            // Arrange
            string username = "username";
            string pan = "1234-1234-1234-1234";
            var paymentRef = "1234";
            var cardHolder = "Oscar Valente";
            var expiryDate = "01-01-2023";
            var cvv = "323";
            var amount = 55;
            var currencyCode = "EUR";
            Merchant merchant = new Merchant
            {
                Id = 1,
                Username = "merchantA",
                Address = "address",
                PasswordHash = "default",
                PasswordSalt = "default",
                IsVerified = true,
                CreatedAt = DateTime.Now
            };

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Authorization"] = "token";

            var payload = new PaymentsPayload
            {
                cardHolder = cardHolder,
                pan = pan,
                expiryDate = expiryDate,
                cvv = cvv,
                amount = amount,
                currencyCode = currencyCode
            };

            var mockToken = new Mock<IToken>();
            mockToken.Setup(t => t.verifyToken("token"))
            .Returns("token");
            var mockCurrencyValidator = new Mock<ICurrencyValidator>();
            mockCurrencyValidator.Setup(cv => cv.isCurrencySupported("eur"))
            .Returns(false);
            var mockMerchantService = new Mock<IMerchant>();
            mockMerchantService.Setup(ms => ms.getMerchantByUsername(username))
            .Returns((Merchant)null);
            var mockPaymentService = new Mock<IPayment>();
            mockPaymentService.Setup(ps => ps.pay(merchant, paymentRef, payload.cardHolder, "1234123412341234", DateOnly.Parse(payload.expiryDate), payload.cvv, payload.amount, payload.currencyCode))
            .Returns("");
            var controller = new PaymentController(mockToken.Object, mockCurrencyValidator.Object, mockMerchantService.Object, mockPaymentService.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = httpContext
                }
            };

            // Act
            var result = (await controller.Pay(payload)).Result as ObjectResult;

            // Assert
            Assert.Equal(403, result.StatusCode);
            Assert.Equal("Merchant is not authorized to issue payment", result.Value);
        }

        [Theory]
        [InlineData("Oscar")]
        [InlineData("Oscar ")]
        [InlineData("Oscar 12345")]
        [InlineData("Oscar V!")]
        public async Task Payment_Fail_CardHolder_Format(string cardHolder)
        {
            // Arrange
            string username = "username";
            string pan = "1234-1234-1234-1234";
            var paymentRef = "1234";
            var expiryDate = "01-01-2023";
            var cvv = "323";
            var amount = 55;
            var currencyCode = "EUR";
            Merchant merchant = new Merchant
            {
                Id = 1,
                Username = "merchantA",
                Address = "address",
                PasswordHash = "default",
                PasswordSalt = "default",
                IsVerified = true,
                CreatedAt = DateTime.Now
            };

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Authorization"] = "token";

            var payload = new PaymentsPayload
            {
                cardHolder = cardHolder,
                pan = pan,
                expiryDate = expiryDate,
                cvv = cvv,
                amount = amount,
                currencyCode = currencyCode
            };

            var mockToken = new Mock<IToken>();
            mockToken.Setup(t => t.verifyToken("token"))
            .Returns(username);
            var mockCurrencyValidator = new Mock<ICurrencyValidator>();
            mockCurrencyValidator.Setup(cv => cv.isCurrencySupported("eur"))
            .Returns(false);
            var mockMerchantService = new Mock<IMerchant>();
            mockMerchantService.Setup(ms => ms.getMerchantByUsername(username))
            .Returns(merchant);
            var mockPaymentService = new Mock<IPayment>();
            mockPaymentService.Setup(ps => ps.pay(merchant, paymentRef, payload.cardHolder, "1234123412341234", DateOnly.Parse(payload.expiryDate), payload.cvv, payload.amount, payload.currencyCode))
            .Returns("");
            var controller = new PaymentController(mockToken.Object, mockCurrencyValidator.Object, mockMerchantService.Object, mockPaymentService.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = httpContext
                }
            };

            // Act
            var result = (await controller.Pay(payload)).Result as ObjectResult;

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid card holder format", result.Value);
        }

        [Theory]
        [InlineData("invalidDate")]
        [InlineData("01")]
        [InlineData("a2032")]
        public async Task Payment_Fail_InvalidExpiryDate_Format(string expiryDate)
        {
            // Arrange
            string username = "username";
            string pan = "1234-1234-1234-1234";
            var cardHolder = "Oscar Valente";
            var paymentRef = "1234";
            var cvv = "323";
            var amount = 55;
            var currencyCode = "EUR";
            Merchant merchant = new Merchant
            {
                Id = 1,
                Username = "merchantA",
                Address = "address",
                PasswordHash = "default",
                PasswordSalt = "default",
                IsVerified = true,
                CreatedAt = DateTime.Now
            };

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Authorization"] = "token";

            var payload = new PaymentsPayload
            {
                cardHolder = cardHolder,
                pan = pan,
                expiryDate = expiryDate,
                cvv = cvv,
                amount = amount,
                currencyCode = currencyCode
            };

            var mockToken = new Mock<IToken>();
            mockToken.Setup(t => t.verifyToken("token"))
            .Returns(username);
            var mockCurrencyValidator = new Mock<ICurrencyValidator>();
            mockCurrencyValidator.Setup(cv => cv.isCurrencySupported("eur"))
            .Returns(false);
            var mockMerchantService = new Mock<IMerchant>();
            mockMerchantService.Setup(ms => ms.getMerchantByUsername(username))
            .Returns(merchant);
            var mockPaymentService = new Mock<IPayment>();
            mockPaymentService.Setup(ps => ps.pay(merchant, paymentRef, payload.cardHolder, "1234123412341234", new DateOnly(2020, 1, 1), payload.cvv, payload.amount, payload.currencyCode))
            .Returns("");
            var controller = new PaymentController(mockToken.Object, mockCurrencyValidator.Object, mockMerchantService.Object, mockPaymentService.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = httpContext
                }
            };

            // Act
            var result = (await controller.Pay(payload)).Result as ObjectResult;

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid expiry date", result.Value);
        }

        [Fact]
        public async Task Payment_Success_AcceptedPayment_NotSaved()
        {
            // Arrange
            string username = "username";
            string pan = "1234-1234-1234-1234";
            var cardHolder = "Oscar Valente";
            var paymentRef = "1234";
            var expiryDate = "01-01-2023";
            var cvv = "323";
            var amount = 55;
            var currencyCode = "EUR";
            Merchant merchant = new Merchant
            {
                Id = 1,
                Username = "merchantA",
                Address = "address",
                PasswordHash = "default",
                PasswordSalt = "default",
                IsVerified = true,
                CreatedAt = DateTime.Now
            };

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Authorization"] = "token";

            var payload = new PaymentsPayload
            {
                cardHolder = cardHolder,
                pan = pan,
                expiryDate = expiryDate,
                cvv = cvv,
                amount = amount,
                currencyCode = currencyCode
            };

            var mockToken = new Mock<IToken>();
            mockToken.Setup(t => t.verifyToken("token"))
            .Returns(username);
            var mockCurrencyValidator = new Mock<ICurrencyValidator>();
            mockCurrencyValidator.Setup(cv => cv.isCurrencySupported(currencyCode))
            .Returns(true);
            var mockMerchantService = new Mock<IMerchant>();
            mockMerchantService.Setup(ms => ms.getMerchantByUsername(username))
            .Returns(merchant);
            var mockPaymentService = new Mock<IPayment>();
            mockPaymentService.Setup(ps => ps.pay(merchant, It.IsAny<string>(), payload.cardHolder, "1234123412341234", DateOnly.Parse(payload.expiryDate), payload.cvv, payload.amount, payload.currencyCode))
            .Throws(new PaymentException("error saving payment", PaymentExceptionCode.ERROR_SAVING_PAYMENT, paymentRef, true));
            var controller = new PaymentController(mockToken.Object, mockCurrencyValidator.Object, mockMerchantService.Object, mockPaymentService.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = httpContext
                }
            };

            // Act
            var result = (await controller.Pay(payload)).Result as ObjectResult;

            // Assert
            Assert.IsType<OkObjectResult>(result);
            Assert.Equal($"{paymentRef} - your payment was accepted but it's still not available for status check", result.Value);
        }

        [Fact]
        public async Task Payment_Fail_RejectedPayment_NotSaved()
        {
            // Arrange
            string username = "username";
            string pan = "1234-1234-1234-1234";
            var cardHolder = "Oscar Valente";
            var paymentRef = "1234";
            var expiryDate = "01-01-2023";
            var cvv = "323";
            var amount = 55;
            var currencyCode = "EUR";
            Merchant merchant = new Merchant
            {
                Id = 1,
                Username = "merchantA",
                Address = "address",
                PasswordHash = "default",
                PasswordSalt = "default",
                IsVerified = true,
                CreatedAt = DateTime.Now
            };

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Authorization"] = "token";

            var payload = new PaymentsPayload
            {
                cardHolder = cardHolder,
                pan = pan,
                expiryDate = expiryDate,
                cvv = cvv,
                amount = amount,
                currencyCode = currencyCode
            };

            var mockToken = new Mock<IToken>();
            mockToken.Setup(t => t.verifyToken("token"))
            .Returns(username);
            var mockCurrencyValidator = new Mock<ICurrencyValidator>();
            mockCurrencyValidator.Setup(cv => cv.isCurrencySupported(currencyCode))
            .Returns(true);
            var mockMerchantService = new Mock<IMerchant>();
            mockMerchantService.Setup(ms => ms.getMerchantByUsername(username))
            .Returns(merchant);
            var mockPaymentService = new Mock<IPayment>();
            mockPaymentService.Setup(ps => ps.pay(merchant, It.IsAny<string>(), payload.cardHolder, "1234123412341234", DateOnly.Parse(payload.expiryDate), payload.cvv, payload.amount, payload.currencyCode))
            .Throws(new PaymentException("error saving payment", PaymentExceptionCode.ERROR_SAVING_PAYMENT, paymentRef, false));
            var controller = new PaymentController(mockToken.Object, mockCurrencyValidator.Object, mockMerchantService.Object, mockPaymentService.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = httpContext
                }
            };

            // Act
            var result = (await controller.Pay(payload)).Result as ObjectResult;

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal($"Payment rejected due to: error saving payment - reference {paymentRef} (payment not yet available for status check)", result.Value);
        }

        // TODO: complete test suite
    }
}
