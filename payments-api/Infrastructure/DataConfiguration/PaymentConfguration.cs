using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentsAPI.Entities;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments");

        builder.HasIndex(e => e.MerchantId, "merchant_id");

        builder.HasIndex(e => e.RefUuid, "ref_uuid")
            .IsUnique();

        builder.Property(e => e.Id).HasColumnName("id");

        builder.Property(e => e.AcquiringBankSwift)
            .HasMaxLength(11)
            .HasColumnName("acquiring_bank_swift");

        builder.Property(e => e.Amount)
            .HasPrecision(5, 2)
            .HasColumnName("amount");

        builder.Property(e => e.CardHolder)
            .HasMaxLength(30)
            .HasColumnName("card_holder");

        builder.Property(e => e.CreatedAt)
            .HasColumnType("timestamp")
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(e => e.CurrencyCode)
            .HasMaxLength(3)
            .HasColumnName("currency_code");

        builder.Property(e => e.ExpiryDate).HasColumnName("expiry_date");

        builder.Property(e => e.IsAccepted).HasColumnName("is_accepted");

        builder.Property(e => e.MerchantId).HasColumnName("merchant_id");

        builder.Property(e => e.Pan)
            .HasMaxLength(16)
            .HasColumnName("pan");

        builder.Property(e => e.RefUuid)
            .HasMaxLength(36)
            .HasColumnName("ref_uuid");

        builder.HasOne(d => d.Merchant)
            .WithMany(p => p.Payments)
            .HasForeignKey(d => d.MerchantId)
            .HasConstraintName("payments_ibfk_1");
    }
}
