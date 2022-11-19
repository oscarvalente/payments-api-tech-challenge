using System;
using System.Collections.Generic;

namespace PaymentsAPI.Entities
{
    public partial class Payment
    {
        public int Id { get; set; }
        public int? MerchantId { get; set; }
        public string RefUuid { get; set; } = null!;
        public decimal Amount { get; set; }
        public string CurrencyCode { get; set; } = null!;
        public string CardHolder { get; set; } = null!;
        public string Pan { get; set; } = null!;
        public DateOnly ExpiryDate { get; set; }
        public string AcquiringBankSwift { get; set; } = null!;
        public bool IsAccepted { get; set; }
        public DateTime? CreatedAt { get; set; }

        public virtual Merchant? Merchant { get; set; }
    }
}
