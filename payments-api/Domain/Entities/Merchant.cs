namespace PaymentsAPI.Entities
{
    public partial class Merchant
    {
        public Merchant()
        {
            Payments = new HashSet<Payment>();
        }

        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string PasswordSalt { get; set; } = null!;
        public bool? IsVerified { get; set; }
        public DateTime? CreatedAt { get; set; }

        public virtual ICollection<Payment> Payments { get; set; }
    }
}
