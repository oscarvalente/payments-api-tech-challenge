using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using PaymentsAPI.Entities;

namespace PaymentsAPI.EfStructures
{
    public partial class PaymentsAPIDbContext : DbContext
    {
        public PaymentsAPIDbContext()
        {
        }

        public PaymentsAPIDbContext(DbContextOptions<PaymentsAPIDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Merchant> Merchants { get; set; } = null!;
        public virtual DbSet<Payment> Payments { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseMySql("server=localhost;user=oscar;password=gitCheckout2022!;database=payments_api", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.31-mysql"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseCollation("utf8mb4_0900_ai_ci")
                .HasCharSet("utf8mb4");

            modelBuilder.Entity<Merchant>(entity =>
            {
                entity.ToTable("merchants");

                entity.HasIndex(e => e.Username, "username")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Address)
                    .HasMaxLength(90)
                    .HasColumnName("address");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("timestamp")
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.IsVerified)
                    .HasColumnName("is_verified")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.PasswordHash)
                    .HasMaxLength(88)
                    .HasColumnName("password_hash");

                entity.Property(e => e.PasswordSalt)
                    .HasMaxLength(383)
                    .HasColumnName("password_salt");

                entity.Property(e => e.Username)
                    .HasMaxLength(20)
                    .HasColumnName("username");
            });

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.ToTable("payments");

                entity.HasIndex(e => e.MerchantId, "merchant_id");

                entity.HasIndex(e => e.RefUuid, "ref_uuid")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.AcquiringBankSwift)
                    .HasMaxLength(11)
                    .HasColumnName("acquiring_bank_swift");

                entity.Property(e => e.Amount)
                    .HasPrecision(5, 2)
                    .HasColumnName("amount");

                entity.Property(e => e.CardHolder)
                    .HasMaxLength(30)
                    .HasColumnName("card_holder");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("timestamp")
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.CurrencyCode)
                    .HasMaxLength(3)
                    .HasColumnName("currency_code");

                entity.Property(e => e.ExpiryDate).HasColumnName("expiry_date");

                entity.Property(e => e.IsAccepted).HasColumnName("is_accepted");

                entity.Property(e => e.MerchantId).HasColumnName("merchant_id");

                entity.Property(e => e.Pan)
                    .HasMaxLength(16)
                    .HasColumnName("pan");

                entity.Property(e => e.RefUuid)
                    .HasMaxLength(36)
                    .HasColumnName("ref_uuid");

                entity.HasOne(d => d.Merchant)
                    .WithMany(p => p.Payments)
                    .HasForeignKey(d => d.MerchantId)
                    .HasConstraintName("payments_ibfk_1");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
