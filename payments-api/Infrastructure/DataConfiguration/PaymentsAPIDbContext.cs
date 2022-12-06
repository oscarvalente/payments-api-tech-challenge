using System;
using System.Collections.Generic;
using System.Reflection;
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
                optionsBuilder.UseMySql("name=ConnectionStrings:Default", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.31-mysql"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
