using MediatR;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PaymentsAPI.Controllers.Payments;
using PaymentsAPI.Entities;
using PaymentsAPI.Services;
using PaymentsAPI.Services.Responses;

namespace unit_tests
{
    public class PaymentControllerTest
    {
        [Fact]
        public async Task Payment_Success_AcceptedPayment_NotSaved()
        {
            // Arrange
            string pan = "1234-1234-1234-1234";
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
            }; ;

            var commandPayload = new CreatePaymentCommand
            {
                CardHolder = cardHolder,
                Pan = pan,
                ExpiryDate = expiryDate,
                Cvv = cvv,
                Amount = amount,
                CurrencyCode = currencyCode
            };
            string paymRef = null;
            var mockAPIResponseBuilder = new Mock<IAPIResponseBuilder>();
            var mockMediatorSender = new Mock<ISender>();
            mockAPIResponseBuilder.Setup(arb => arb.buildPaymentRefResponse(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((string paymentRef, string msg) =>
            {
                paymRef = paymentRef;
                return new PaymentRefResponse
                {
                    PaymentRef = paymentRef,
                    Message = msg
                };
            });
            var controller = new PaymentController(mockAPIResponseBuilder.Object)
            {
                // Mediator = mockMediatorSender.Object
            };

            // Act
            var result = (await controller.Pay(commandPayload));

            // Assert
            Assert.IsType<CreatedResult>(result);
            Assert.Equal(JsonConvert.SerializeObject(new
            {
                PaymentRef = paymRef,
                Message = "Your payment was accepted but it's still not available for status check"
            }), JsonConvert.SerializeObject(result));
        }

        // TODO: complete test suite
    }
}
