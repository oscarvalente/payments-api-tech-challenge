using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Newtonsoft.Json;
using PaymentsAPI.Controllers.Payments;
using PaymentsAPI.Entities;
using PaymentsAPI.Errors;
using PaymentsAPI.Services;
using PaymentsAPI.Services.Responses;

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

            var payload = new PaymentModel
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
            var mockAPIResponseBuilder = new Mock<IAPIResponseBuilder>();
            mockAPIResponseBuilder.Setup(arb => arb.buildClientError(It.IsAny<string>(), It.IsAny<string>()))
            // return the exact same parameters with which it was invoked
            .Returns((string code, string msg) => new APIError
            {
                Code = code,
                Message = msg
            });
            var controller = new PaymentController(mockToken.Object, mockCurrencyValidator.Object, mockMerchantService.Object, mockPaymentService.Object, mockAPIResponseBuilder.Object)
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
            Assert.Equal(JsonConvert.SerializeObject(new
            {
                Code = "UNAUTHORIZED_ACCESS",
                Message = "Failed to authenticate"
            }), JsonConvert.SerializeObject(result.Value));
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

            var payload = new PaymentModel
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
            var mockAPIResponseBuilder = new Mock<IAPIResponseBuilder>();
            mockAPIResponseBuilder.Setup(arb => arb.buildClientError(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((string code, string msg) => new APIError
            {
                Code = code,
                Message = msg
            });
            var controller = new PaymentController(mockToken.Object, mockCurrencyValidator.Object, mockMerchantService.Object, mockPaymentService.Object, mockAPIResponseBuilder.Object)
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
            Assert.Equal(JsonConvert.SerializeObject(new
            {
                Code = "UNAUTHORIZED_ACCESS",
                Message = "Failed to authenticate"
            }), JsonConvert.SerializeObject(result.Value));
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

            var payload = new PaymentModel
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
            var mockAPIResponseBuilder = new Mock<IAPIResponseBuilder>();
            mockAPIResponseBuilder.Setup(arb => arb.buildClientError(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((string code, string msg) => new APIError
            {
                Code = code,
                Message = msg
            });
            var controller = new PaymentController(mockToken.Object, mockCurrencyValidator.Object, mockMerchantService.Object, mockPaymentService.Object, mockAPIResponseBuilder.Object)
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
            Assert.Equal(JsonConvert.SerializeObject(new
            {
                Code = "UNAUTHORIZED_ACCESS",
                Message = "Merchant is not authorized to issue payment"
            }), JsonConvert.SerializeObject(result.Value));
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

            var payload = new PaymentModel
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
            mockPaymentService.Setup(ps => ps.pay(merchant, paymentRef, payload.cardHolder, "1234123412341234", new DateOnly(2020, 1, 1), payload.cvv, payload.amount, payload.currencyCode))
            .Returns("");
            var mockAPIResponseBuilder = new Mock<IAPIResponseBuilder>();
            mockAPIResponseBuilder.Setup(arb => arb.buildClientError(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((string code, string msg) => new APIError
            {
                Code = code,
                Message = msg
            });
            var controller = new PaymentController(mockToken.Object, mockCurrencyValidator.Object, mockMerchantService.Object, mockPaymentService.Object, mockAPIResponseBuilder.Object)
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
            Assert.Equal(JsonConvert.SerializeObject(new
            {
                Code = "INVALID_FORMAT_EXPIRY_DATE",
                Message = "Invalid expiry date"
            }), JsonConvert.SerializeObject(result.Value));
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

            var payload = new PaymentModel
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

            string PaymentRef = null;
            var mockAPIResponseBuilder = new Mock<IAPIResponseBuilder>();
            mockAPIResponseBuilder.Setup(arb => arb.buildPaymentRefResponse(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((string paymentRef, string msg) =>
            {
                PaymentRef = paymentRef;
                return new PaymentRefResponse
                {
                    PaymentRef = paymentRef,
                    Message = msg
                };
            });
            var controller = new PaymentController(mockToken.Object, mockCurrencyValidator.Object, mockMerchantService.Object, mockPaymentService.Object, mockAPIResponseBuilder.Object)
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
            Assert.Equal(JsonConvert.SerializeObject(new
            {
                PaymentRef = PaymentRef,
                Message = "Your payment was accepted but it's still not available for status check"
            }), JsonConvert.SerializeObject(result.Value));
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

            var payload = new PaymentModel
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

            string PaymentRef = null;
            var mockAPIResponseBuilder = new Mock<IAPIResponseBuilder>();
            mockAPIResponseBuilder.Setup(arb => arb.buildClientErrorWithPaymentRef(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns((string code, string msg, string paymentRef) =>
            {
                PaymentRef = paymentRef;
                return new APIErrorPaymentRef
                {
                    Code = code,
                    Message = msg,
                    PaymentRef = paymentRef
                };
            });
            var controller = new PaymentController(mockToken.Object, mockCurrencyValidator.Object, mockMerchantService.Object, mockPaymentService.Object, mockAPIResponseBuilder.Object)
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
            Assert.Equal(JsonConvert.SerializeObject(new
            {
                Code = "ERROR_SAVING_PAYMENT",
                Message = $"Payment rejected due to: error saving payment - reference {paymentRef} (payment not yet available for status check)",
                PaymentRef = PaymentRef
            }), JsonConvert.SerializeObject(result.Value));
        }

        // TODO: complete test suite
    }
}
