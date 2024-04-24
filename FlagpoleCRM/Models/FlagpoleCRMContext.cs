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
        public virtual DbSet<Template> Templates { get; set; } = null!;
        public virtual DbSet<UnsubcribedEmail> UnsubcribedEmails { get; set; } = null!;
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

                entity.HasOne(a => a.Audience)
                    .WithMany(x => x.AudienceCustomers)
                    .HasForeignKey(a => a.AudienceId);
            });

            modelBuilder.Entity<Campaign>(entity =>
            {
                entity.ToTable("Campaign");
                entity.HasOne(e => e.Email)
                    .WithMany(c => c.Campaigns)
                    .HasForeignKey(e => e.EmailId);
                entity.HasOne(p => p.Phone)
                    .WithMany(c => c.Campaigns)
                    .HasForeignKey(p => p.PhoneId);
                entity.HasOne(w => w.Website)
                    .WithMany(c => c.Campaigns)
                    .HasForeignKey(w => w.WebsiteId);
                entity.HasOne(t => t.Template)
                    .WithMany(c => c.Campaigns)
                    .HasForeignKey(t => t.TemplateId);
                entity.HasOne(a => a.Audience)
                    .WithMany(c => c.Campaigns)
                    .HasForeignKey(a => a.AudienceId);
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
                entity.HasOne(a => a.Account)
                    .WithMany(w => w.Websites)
                    .HasForeignKey(a => a.AccountId);
            });

            modelBuilder.Entity<Template>(entity =>
            {
                entity.ToTable("Template");
            });

            modelBuilder.Entity<UnsubcribedEmail>(entity =>
            {
                entity.ToTable("UnsubcribedEmail");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
