using Newtonsoft.Json;
using PaymentsAPI.Banks.Services;
using PaymentsAPI.Entities;
using PaymentsAPI.Errors;
using PaymentsAPI.Payments.Services;
using PaymentsAPI.Utils;
using PaymentsAPI.Web.Responses;

namespace unit_tests
{
    public class PaymentsServiceTest
    {
        [Fact]
        public void Payment_Fail_AcquiringBankNotSupported()
        {
            // Arrange
            string pan = "1234-1234-1234-1234";
            var cardHolder = "Oscar";
            var expiryDate = new DateOnly(2032, 12, 31);
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

            var mockBankMatcher = new Mock<IBankMatcher>();
            mockBankMatcher.Setup(bankMatcher => bankMatcher.loadBankByPAN(pan))
            .Returns((IDummyAcquiringBank)null);
            var mockPaymentData = new Mock<IPaymentData>();
            mockPaymentData.Setup(paymentData => paymentData.addPayment("1234", 55, "EUR", "Oscar", pan, expiryDate, "swift", true, merchant))
            .Returns((Payment)null);
            var payments = new Payments(mockBankMatcher.Object, mockPaymentData.Object);


            // Act
            Action act = () => payments.pay(merchant, cardHolder, pan, expiryDate, cvv, amount, currencyCode);
            PaymentException exception = Assert.Throws<PaymentException>(act);

            // Assert
            Assert.Equal("Acquiring bank is not supported", exception.Message);
            Assert.Equal(PaymentExceptionCode.BANK_NOT_SUPPORTED, exception.code);
        }

        [Fact]
        public void Payment_Fail_BankPaymentProcessing()
        {
            // Arrange
            string pan = "1234-1234-1234-1234";
            var paymentRef = "1234";
            var cardHolder = "Oscar";
            var expiryDate = new DateOnly(2032, 12, 31);
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

            var mockBankMatcher = new Mock<IBankMatcher>();
            var mockDummyBankA = new Mock<IDummyAcquiringBank>();
            mockDummyBankA.Setup(dummyBankA => dummyBankA.isValidPayment(pan, cardHolder, expiryDate, cvv, amount))
            .Throws(new Exception());
            mockBankMatcher.Setup(bankMatcher => bankMatcher.loadBankByPAN(pan))
            .Returns(mockDummyBankA.Object);
            var mockPaymentData = new Mock<IPaymentData>();
            mockPaymentData.Setup(paymentData => paymentData.addPayment("1234", 55, "EUR", "Oscar", pan, expiryDate, "swift", true, merchant))
            .Returns((Payment)null);
            var payments = new Payments(mockBankMatcher.Object, mockPaymentData.Object);


            // Act
            Action act = () => payments.pay(merchant, cardHolder, pan, expiryDate, cvv, amount, currencyCode);
            PaymentException exception = Assert.Throws<PaymentException>(act);

            // Assert
            mockDummyBankA.Verify(db => db.isValidPayment(pan, cardHolder, expiryDate, cvv, amount), Times.Once);
            Assert.Equal("Error communicating payment with acquiring bank", exception.Message);
            Assert.Equal(PaymentExceptionCode.BANK_PAYMENT_PROCESSING, exception.code);
        }

        [Fact]
        public void Payment_Fail_RejectedPayment()
        {
            // Arrange
            string pan = "1234-1234-1234-1234";
            var paymentRef = "1234";
            var cardHolder = "Oscar";
            var expiryDate = new DateOnly(2032, 12, 31);
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

            var mockBankMatcher = new Mock<IBankMatcher>();
            var mockDummyBankA = new Mock<IDummyAcquiringBank>();
            mockDummyBankA.Setup(dummyBankA => dummyBankA.isValidPayment(pan, cardHolder, expiryDate, cvv, amount))
            .Returns(false);
            mockDummyBankA.Setup(dummyBankA => dummyBankA.getSwiftCode())
            .Returns("swift");
            mockBankMatcher.Setup(bankMatcher => bankMatcher.loadBankByPAN(pan))
            .Returns(mockDummyBankA.Object);
            var payment = new Payment
            {
                Id = 1,
                MerchantId = 2,
                RefUuid = paymentRef,
                Amount = amount,
                CurrencyCode = currencyCode,
                CardHolder = cardHolder,
                Pan = pan,
                ExpiryDate = expiryDate,
                AcquiringBankSwift = "swift",
                IsAccepted = false,
                CreatedAt = DateTime.Now
            };
            var mockPaymentData = new Mock<IPaymentData>();
            mockPaymentData.Setup(paymentData => paymentData.addPayment(It.IsAny<string>(), amount, currencyCode, "OSCAR", pan, expiryDate, "swift", false, merchant))
            .Returns((Payment)payment);
            var payments = new Payments(mockBankMatcher.Object, mockPaymentData.Object);

            // Act
            string result = payments.pay(merchant, cardHolder, pan, expiryDate, cvv, amount, currencyCode);

            // Assert
            mockDummyBankA.Verify(db => db.isValidPayment(pan, cardHolder, expiryDate, cvv, amount), Times.Once);
            mockPaymentData.Verify(pd => pd.addPayment(It.IsAny<string>(), amount, currencyCode, "OSCAR", pan, expiryDate, "swift", false, merchant), Times.Once);
            Assert.Equal(paymentRef, result);
        }

        [Fact]
        public void Payment_Fail_RejectedPayment_NotSaved()
        {
            // Arrange
            string pan = "1234-1234-1234-1234";
            var paymentRef = "1234";
            var cardHolder = "Oscar";
            var expiryDate = new DateOnly(2032, 12, 31);
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

            var mockBankMatcher = new Mock<IBankMatcher>();
            var mockDummyBankA = new Mock<IDummyAcquiringBank>();
            mockDummyBankA.Setup(dummyBankA => dummyBankA.isValidPayment(pan, cardHolder, expiryDate, cvv, amount))
            .Returns(false);
            mockDummyBankA.Setup(dummyBankA => dummyBankA.getSwiftCode())
            .Returns("swift");
            mockBankMatcher.Setup(bankMatcher => bankMatcher.loadBankByPAN(pan))
            .Returns(mockDummyBankA.Object);
            var payment = new Payment
            {
                Id = 1,
                MerchantId = 2,
                RefUuid = paymentRef,
                Amount = amount,
                CurrencyCode = currencyCode,
                CardHolder = cardHolder,
                Pan = pan,
                ExpiryDate = expiryDate,
                AcquiringBankSwift = "swift",
                IsAccepted = false,
                CreatedAt = DateTime.Now
            };
            var mockPaymentData = new Mock<IPaymentData>();
            mockPaymentData.Setup(paymentData => paymentData.addPayment(It.IsAny<string>(), amount, currencyCode, "OSCAR", pan, expiryDate, "swift", false, merchant))
            .Throws(new Exception());
            var payments = new Payments(mockBankMatcher.Object, mockPaymentData.Object);

            // Act
            Action act = () => payments.pay(merchant, cardHolder, pan, expiryDate, cvv, amount, currencyCode);
            PaymentException exception = Assert.Throws<PaymentException>(act);

            // Assert
            mockDummyBankA.Verify(db => db.isValidPayment(pan, cardHolder, expiryDate, cvv, amount), Times.Once);
            mockPaymentData.Verify(pd => pd.addPayment(It.IsAny<string>(), amount, currencyCode, "OSCAR", pan, expiryDate, "swift", false, merchant), Times.Once);
            Assert.Equal("Error saving payment to database", exception.Message);
            Assert.Equal(PaymentExceptionCode.ERROR_SAVING_PAYMENT, exception.code);
            Assert.IsType<string>(exception.paymentRef);
            Assert.Equal(false, exception.isAccepted);
        }

        [Fact]
        public void Payment_Fail_AcceptedPayment_NotSaved()
        {
            // Arrange
            string pan = "1234-1234-1234-1234";
            var paymentRef = "1234";
            var cardHolder = "Oscar";
            var expiryDate = new DateOnly(2032, 12, 31);
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

            var mockBankMatcher = new Mock<IBankMatcher>();
            var mockDummyBankA = new Mock<IDummyAcquiringBank>();
            mockDummyBankA.Setup(dummyBankA => dummyBankA.isValidPayment(pan, cardHolder, expiryDate, cvv, amount))
            .Returns(true);
            mockDummyBankA.Setup(dummyBankA => dummyBankA.getSwiftCode())
            .Returns("swift");
            mockBankMatcher.Setup(bankMatcher => bankMatcher.loadBankByPAN(pan))
            .Returns(mockDummyBankA.Object);
            var payment = new Payment
            {
                Id = 1,
                MerchantId = 2,
                RefUuid = paymentRef,
                Amount = amount,
                CurrencyCode = currencyCode,
                CardHolder = cardHolder,
                Pan = pan,
                ExpiryDate = expiryDate,
                AcquiringBankSwift = "swift",
                IsAccepted = true,
                CreatedAt = DateTime.Now
            };
            var mockPaymentData = new Mock<IPaymentData>();
            mockPaymentData.Setup(paymentData => paymentData.addPayment(It.IsAny<string>(), amount, currencyCode, "OSCAR", pan, expiryDate, "swift", true, merchant))
            .Throws(new Exception());
            var payments = new Payments(mockBankMatcher.Object, mockPaymentData.Object);

            // Act
            Action act = () => payments.pay(merchant, cardHolder, pan, expiryDate, cvv, amount, currencyCode);
            PaymentException exception = Assert.Throws<PaymentException>(act);

            // Assert
            mockDummyBankA.Verify(db => db.isValidPayment(pan, cardHolder, expiryDate, cvv, amount), Times.Once);
            mockPaymentData.Verify(pd => pd.addPayment(It.IsAny<string>(), amount, currencyCode, "OSCAR", pan, expiryDate, "swift", true, merchant), Times.Once);
            Assert.Equal("Error saving payment to database", exception.Message);
            Assert.Equal(PaymentExceptionCode.ERROR_SAVING_PAYMENT, exception.code);
            Assert.IsType<string>(exception.paymentRef);
            Assert.Equal(true, exception.isAccepted);
        }

        [Fact]
        public void Payment_Success_AcceptedPayment()
        {
            // Arrange
            string pan = "1234-1234-1234-1234";
            var paymentRef = "1234";
            var cardHolder = "Oscar";
            var expiryDate = new DateOnly(2032, 12, 31);
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

            var mockBankMatcher = new Mock<IBankMatcher>();
            var mockDummyBankA = new Mock<IDummyAcquiringBank>();
            mockDummyBankA.Setup(dummyBankA => dummyBankA.isValidPayment(pan, cardHolder, expiryDate, cvv, amount))
            .Returns(true);
            mockDummyBankA.Setup(dummyBankA => dummyBankA.getSwiftCode())
            .Returns("swift");
            mockBankMatcher.Setup(bankMatcher => bankMatcher.loadBankByPAN(pan))
            .Returns(mockDummyBankA.Object);
            var payment = new Payment
            {
                Id = 1,
                MerchantId = 2,
                RefUuid = paymentRef,
                Amount = amount,
                CurrencyCode = currencyCode,
                CardHolder = cardHolder,
                Pan = pan,
                ExpiryDate = expiryDate,
                AcquiringBankSwift = "swift",
                IsAccepted = true,
                CreatedAt = DateTime.Now
            };
            var mockPaymentData = new Mock<IPaymentData>();
            mockPaymentData.Setup(paymentData => paymentData.addPayment(It.IsAny<string>(), amount, currencyCode, "OSCAR", pan, expiryDate, "swift", true, merchant))
            .Returns((Payment)payment);
            var payments = new Payments(mockBankMatcher.Object, mockPaymentData.Object);

            // Act
            string result = payments.pay(merchant, cardHolder, pan, expiryDate, cvv, amount, currencyCode);

            // Assert
            mockDummyBankA.Verify(db => db.isValidPayment(pan, cardHolder, expiryDate, cvv, amount), Times.Once);
            mockPaymentData.Verify(pd => pd.addPayment(It.IsAny<string>(), amount, currencyCode, "OSCAR", pan, expiryDate, "swift", true, merchant), Times.Once);
            Assert.Equal(paymentRef, result);
        }

        [Fact]
        public void GetPayment_Fail_NotFound()
        {
            // Arrange
            var paymentRef = "1234";
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
            string pan = "1234-1234-1234-1234";
            var cardHolder = "Oscar";
            var expiryDate = new DateOnly(2032, 12, 31);
            var cvv = "323";
            var amount = 55;
            var currencyCode = "EUR";
            var payment = new Payment
            {
                Id = 1,
                MerchantId = 2,
                RefUuid = paymentRef,
                Amount = amount,
                CurrencyCode = currencyCode,
                CardHolder = cardHolder,
                Pan = pan,
                ExpiryDate = expiryDate,
                AcquiringBankSwift = "swift",
                IsAccepted = true,
                CreatedAt = DateTime.Now
            };

            var mockBankMatcher = new Mock<IBankMatcher>();
            var mockDummyBankA = new Mock<IDummyAcquiringBank>();
            var mockPaymentData = new Mock<IPaymentData>();
            mockPaymentData.Setup(paymentData => paymentData.getPaymentByRefUUID(paymentRef))
            .Returns((Payment)null);
            var payments = new Payments(mockBankMatcher.Object, mockPaymentData.Object);

            // Act
            Action act = () => payments.getPaymentByRef(paymentRef, merchant);
            PaymentException exception = Assert.Throws<PaymentException>(act);

            // Assert
            mockPaymentData.Verify(pd => pd.getPaymentByRefUUID(paymentRef), Times.Once);
            Assert.Equal(PaymentExceptionCode.PAYMENT_NOT_FOUND, exception.code);
            Assert.Equal("Payment retrieval not authorized", exception.Message);
        }

        [Fact]
        public void GetPayment_Fail_UnauthorizedMerchant()
        {
            // Arrange
            var paymentRef = "1234";
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
            string pan = "1234-1234-1234-1234";
            var cardHolder = "Oscar";
            var expiryDate = new DateOnly(2032, 12, 31);
            var cvv = "323";
            var amount = 55;
            var currencyCode = "EUR";
            var payment = new Payment
            {
                Id = 1,
                MerchantId = 2,
                RefUuid = paymentRef,
                Amount = amount,
                CurrencyCode = currencyCode,
                CardHolder = cardHolder,
                Pan = pan,
                ExpiryDate = expiryDate,
                AcquiringBankSwift = "swift",
                IsAccepted = true,
                CreatedAt = DateTime.Now
            };

            var mockBankMatcher = new Mock<IBankMatcher>();
            var mockDummyBankA = new Mock<IDummyAcquiringBank>();
            var mockPaymentData = new Mock<IPaymentData>();
            mockPaymentData.Setup(paymentData => paymentData.getPaymentByRefUUID(paymentRef))
            .Returns(payment);
            var payments = new Payments(mockBankMatcher.Object, mockPaymentData.Object);

            // Act
            Action act = () => payments.getPaymentByRef(paymentRef, merchant);
            PaymentException exception = Assert.Throws<PaymentException>(act);

            // Assert
            mockPaymentData.Verify(pd => pd.getPaymentByRefUUID(paymentRef), Times.Once);
            Assert.Equal(PaymentExceptionCode.PAYMENT_RETRIEVAL_NOT_AUTHORIZED, exception.code);
            Assert.Equal("Payment retrieval not authorized", exception.Message);
        }

        [Fact]
        public void GetPayment_Success()
        {
            // Arrange
            var paymentRef = "1234";
            var createdAt = DateTime.Now;
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
            string pan = "1234123412341234"; // raw format
            var cardHolder = "Oscar";
            var expiryDate = new DateOnly(2032, 12, 31);
            var amount = 40.50M;
            var currencyCode = "EUR";
            var payment = new Payment
            {
                Id = 1,
                MerchantId = 1,
                RefUuid = paymentRef,
                Amount = amount,
                CurrencyCode = currencyCode,
                CardHolder = cardHolder,
                Pan = pan,
                ExpiryDate = expiryDate,
                AcquiringBankSwift = "swift",
                IsAccepted = true,
                CreatedAt = createdAt
            };

            var mockBankMatcher = new Mock<IBankMatcher>();
            var mockDummyBankA = new Mock<IDummyAcquiringBank>();
            var mockPaymentData = new Mock<IPaymentData>();
            mockPaymentData.Setup(paymentData => paymentData.getPaymentByRefUUID(paymentRef))
            .Returns(payment);
            var payments = new Payments(mockBankMatcher.Object, mockPaymentData.Object);

            // Act
            var result = payments.getPaymentByRef(paymentRef, merchant);

            // Assert
            mockPaymentData.Verify(pd => pd.getPaymentByRefUUID(paymentRef), Times.Once);
            Assert.Equal(JsonConvert.SerializeObject(payment), JsonConvert.SerializeObject(result));
        }
    }
}
