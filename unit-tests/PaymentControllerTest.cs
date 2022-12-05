using MediatR;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PaymentsAPI.Controllers.Payments;
using PaymentsAPI.Entities;
using PaymentsAPI.Errors;
using PaymentsAPI.Services;
using PaymentsAPI.Services.Responses;

namespace unit_tests
{
    public class PaymentControllerTest
    {
        [Fact]
        public async Task Payment_Success_Created()
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

            string paymentReference = "payment ref";
            var mockMediator = new Mock<IMediator>();
            mockMediator.Setup(x => x.Send(It.IsAny<CreatePaymentCommand>(), default(CancellationToken)))
                        .ReturnsAsync(paymentReference);


            var controller = new PaymentController(mockAPIResponseBuilder.Object, mockMediator.Object);

            // Act
            var result = (await controller.Pay(commandPayload, CancellationToken.None));

            // Assert
            Assert.IsType<CreatedResult>(result);
            Assert.Equal(paymentReference, ((CreatedResult)result).Location);
        }

        [Fact]
        public async Task Payment_Success_Created_NotSaved()
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

            string paymentReference = "payment ref";
            var mockMediator = new Mock<IMediator>();
            mockMediator.Setup(x => x.Send(It.IsAny<CreatePaymentCommand>(), default(CancellationToken)))
                        .ThrowsAsync(new PaymentException("payment not saved", PaymentExceptionCode.ERROR_SAVING_PAYMENT, paymentReference, true));


            var controller = new PaymentController(mockAPIResponseBuilder.Object, mockMediator.Object);

            // Act
            var result = (await controller.Pay(commandPayload, CancellationToken.None));

            // Assert
            Assert.IsType<CreatedResult>(result);
            Assert.Equal(paymentReference, ((CreatedResult)result).Location);
        }

        [Fact]
        public async Task Payment_Fail_NotAccepted()
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

            string paymentReference = "payment ref";
            var mockMediator = new Mock<IMediator>();
            mockMediator.Setup(x => x.Send(It.IsAny<CreatePaymentCommand>(), default(CancellationToken)))
                        .ThrowsAsync(new PaymentException("payment not saved", PaymentExceptionCode.ERROR_SAVING_PAYMENT, paymentReference, false));


            var controller = new PaymentController(mockAPIResponseBuilder.Object, mockMediator.Object);

            // Act
            await Assert.ThrowsAnyAsync<PaymentException>(() => controller.Pay(commandPayload, CancellationToken.None));
        }

        [Fact]
        public async Task Payment_Fail_Other_PaymentException()
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

            string paymentReference = "payment ref";
            var mockMediator = new Mock<IMediator>();
            mockMediator.Setup(x => x.Send(It.IsAny<CreatePaymentCommand>(), default(CancellationToken)))
                        .ThrowsAsync(new PaymentException("payment not saved", PaymentExceptionCode.BANK_NOT_SUPPORTED, paymentReference, true));


            var controller = new PaymentController(mockAPIResponseBuilder.Object, mockMediator.Object);

            // Act
            await Assert.ThrowsAnyAsync<PaymentException>(() => controller.Pay(commandPayload, CancellationToken.None));
        }

        [Fact]
        public async Task Payment_Fail_Other_Exception()
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

            string paymentReference = "payment ref";
            var mockMediator = new Mock<IMediator>();
            mockMediator.Setup(x => x.Send(It.IsAny<CreatePaymentCommand>(), default(CancellationToken)))
                        .ThrowsAsync(new Exception("payment not saved"));


            var controller = new PaymentController(mockAPIResponseBuilder.Object, mockMediator.Object);

            // Act
            await Assert.ThrowsAnyAsync<Exception>(() => controller.Pay(commandPayload, CancellationToken.None));
        }
    }
}
