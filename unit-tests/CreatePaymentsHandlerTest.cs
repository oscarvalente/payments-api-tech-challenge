using Newtonsoft.Json;
using PaymentsAPI.Entities;
using PaymentsAPI.Errors;
using PaymentsAPI.Services;
using PaymentsAPI.Services.Responses;

namespace unit_tests
{
    public class CreatePaymentHandlerTest
    {
        [Fact]
        public async Task Payment_Fail_CurrencyNotSupported()
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

            var commandPayload = new CreatePaymentCommand
            {
                CardHolder = cardHolder,
                Pan = pan,
                ExpiryDate = expiryDate,
                Cvv = cvv,
                Amount = amount,
                CurrencyCode = currencyCode
            };

            var mockCurrencyValidator = new Mock<ICurrencyValidator>();
            mockCurrencyValidator.Setup(cv => cv.isCurrencySupported(currencyCode))
            .Returns(false);
            var mockMerchantService = new Mock<IMerchant>();
            mockMerchantService.Setup(ms => ms.getMerchantByUsername(username))
            .Returns(merchant);
            var mockPaymentService = new Mock<IPayment>();
            mockPaymentService.Setup(ps => ps.pay(merchant, It.IsAny<string>(), commandPayload.CardHolder, "1234123412341234", DateOnly.Parse(commandPayload.ExpiryDate), commandPayload.Cvv, commandPayload.Amount, commandPayload.CurrencyCode))
            .Throws(new PaymentException("error saving payment", PaymentExceptionCode.ERROR_SAVING_PAYMENT, paymentRef, false));
            var mockRequesterMerchant = new Mock<IRequesterMerchant>();
            mockRequesterMerchant.SetupGet(x => x.merchant).Returns(merchant);
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
            var handler = new CreatePaymentHandler(mockCurrencyValidator.Object, mockPaymentService.Object, mockAPIResponseBuilder.Object, mockRequesterMerchant.Object);


            // Act
            Action act = async () => await handler.Handle(commandPayload, CancellationToken.None);
            PaymentException exception = Assert.Throws<PaymentException>(act);

            // Assert
            mockPaymentService.Verify(pd => pd.pay(It.IsAny<Merchant>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateOnly>(), It.IsAny<string>(), It.IsAny<decimal>(), It.IsAny<string>()), Times.Never);
            Assert.Equal(PaymentExceptionCode.CURRENCY_NOT_SUPPORTED, exception.code);
            Assert.Equal("Currency code is not supported", exception.Message);
        }

        public async Task Payment_Success_CurrencySupported()
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

            var commandPayload = new CreatePaymentCommand
            {
                CardHolder = cardHolder,
                Pan = pan,
                ExpiryDate = expiryDate,
                Cvv = cvv,
                Amount = amount,
                CurrencyCode = currencyCode
            };

            var mockCurrencyValidator = new Mock<ICurrencyValidator>();
            mockCurrencyValidator.Setup(cv => cv.isCurrencySupported(currencyCode))
            .Returns(false);
            var mockMerchantService = new Mock<IMerchant>();
            mockMerchantService.Setup(ms => ms.getMerchantByUsername(username))
            .Returns(merchant);
            var mockPaymentService = new Mock<IPayment>();
            mockPaymentService.Setup(ps => ps.pay(merchant, It.IsAny<string>(), commandPayload.CardHolder, "1234123412341234", DateOnly.Parse(commandPayload.ExpiryDate), commandPayload.Cvv, commandPayload.Amount, commandPayload.CurrencyCode))
            .Returns("the payment reference");
            var mockRequesterMerchant = new Mock<IRequesterMerchant>();
            mockRequesterMerchant.SetupGet(x => x.merchant).Returns(merchant);
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
            var handler = new CreatePaymentHandler(mockCurrencyValidator.Object, mockPaymentService.Object, mockAPIResponseBuilder.Object, mockRequesterMerchant.Object);


            // Act
            var result = (await handler.Handle(commandPayload, CancellationToken.None));

            // Assert
            Assert.Equal("the payment reference", result);
        }
    }
}
