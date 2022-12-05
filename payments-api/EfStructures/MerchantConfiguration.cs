using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentsAPI.Entities;

public class MerchantConfiguration : IEntityTypeConfiguration<Merchant>
{
    public void Configure(EntityTypeBuilder<Merchant> builder)
    {
        builder.ToTable("merchants");

        builder.HasIndex(e => e.Username, "username")
            .IsUnique();

        builder.Property(e => e.Id).HasColumnName("id");

        builder.Property(e => e.Address)
            .HasMaxLength(90)
            .HasColumnName("address");

        builder.Property(e => e.CreatedAt)
            .HasColumnType("timestamp")
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(e => e.IsVerified)
            .HasColumnName("is_verified")
            .HasDefaultValueSql("'0'");

        builder.Property(e => e.PasswordHash)
            .HasMaxLength(88)
            .HasColumnName("password_hash");

        builder.Property(e => e.PasswordSalt)
            .HasMaxLength(383)
            .HasColumnName("password_salt");

        builder.Property(e => e.Username)
            .HasMaxLength(20)
            .HasColumnName("username");
    }
}
