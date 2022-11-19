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
        public string Pan { get; set; } = null!;
        public DateOnly ExpiryDate { get; set; }
        public string Ccv { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }

        public virtual Merchant? Merchant { get; set; }
    }
}
