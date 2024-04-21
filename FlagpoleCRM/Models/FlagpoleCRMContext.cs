using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace FlagpoleCRM.Models
{
    public partial class FlagpoleCRMContext : DbContext
    {
        public FlagpoleCRMContext()
        {
        }

        public FlagpoleCRMContext(DbContextOptions<FlagpoleCRMContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Account> Accounts { get; set; } = null!;
        public virtual DbSet<Audience> Audiences { get; set; } = null!;
        public virtual DbSet<AudienceCustomer> AudienceCustomers { get; set; } = null!;
        public virtual DbSet<Campaign> Campaigns { get; set; } = null!;
        public virtual DbSet<CustomerField> CustomerFields { get; set; } = null!;
        public virtual DbSet<EmailAccount> EmailAccounts { get; set; } = null!;
        public virtual DbSet<PhoneAccount> PhoneAccounts { get; set; } = null!;
        public virtual DbSet<Website> Websites { get; set; } = null!;

        public virtual DbSet<Template> Templates { get; set; } = null;
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>(entity =>
            {
                entity.ToTable("Account");
            });

            modelBuilder.Entity<Audience>(entity =>
            {
                entity.ToTable("Audience");
            });

            modelBuilder.Entity<AudienceCustomer>(entity =>
            {
                entity.ToTable("Audience_Customer");
            });

            modelBuilder.Entity<Campaign>(entity =>
            {
                entity.ToTable("Campaign");
            });

            modelBuilder.Entity<CustomerField>(entity =>
            {
                entity.ToTable("CustomerField");
            });

            modelBuilder.Entity<EmailAccount>(entity =>
            {
                entity.ToTable("Email_Account");
            });

            modelBuilder.Entity<PhoneAccount>(entity =>
            {
                entity.ToTable("Phone_Account");
            });

            modelBuilder.Entity<Website>(entity =>
            {
                entity.ToTable("Website");
            });

            modelBuilder.Entity<Template>(entity =>
            {
                entity.ToTable("Template");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
