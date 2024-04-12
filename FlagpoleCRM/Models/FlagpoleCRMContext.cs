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
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>(entity =>
            {
                entity.ToTable("Account");

                entity.Property(e => e.Id)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.Avatar).IsUnicode(false);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.Email)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.FullName).HasMaxLength(200);

                entity.Property(e => e.Password).IsUnicode(false);

                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.SecretKey).IsUnicode(false);

                entity.Property(e => e.Salt).IsUnicode(false);

                entity.Property(e => e.Timezone)
                    .HasMaxLength(200)
                    .IsUnicode(false); ;

                entity.Property(e => e.IsDeleted).HasColumnType("bit");

                entity.Property(e => e.PhoneNumberConfirmed).HasColumnType("bit");

                entity.Property(e => e.EmailConfirmed).HasColumnType("bit");
            });

            modelBuilder.Entity<Audience>(entity =>
            {
                entity.ToTable("Audience");

                entity.Property(e => e.Id)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(200);

                entity.Property(e => e.Sqlquery).HasColumnName("SQLQuery");
            });

            modelBuilder.Entity<AudienceCustomer>(entity =>
            {
                entity.ToTable("Audience_Customer");

                entity.Property(e => e.AudienceId)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.CustomerId)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.HasOne(d => d.Audience)
                    .WithMany(p => p.AudienceCustomers)
                    .HasForeignKey(d => d.AudienceId)
                    .HasConstraintName("FK__Audience___Audie__2B3F6F97");
            });

            modelBuilder.Entity<Campaign>(entity =>
            {
                entity.ToTable("Campaign");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(200);

                entity.Property(e => e.SendDate).HasColumnType("datetime");

                entity.HasOne(d => d.Email)
                    .WithMany(p => p.Campaigns)
                    .HasForeignKey(d => d.EmailId)
                    .HasConstraintName("FK__Campaign__EmailI__36B12243");

                entity.HasOne(d => d.Phone)
                    .WithMany(p => p.Campaigns)
                    .HasForeignKey(d => d.PhoneId)
                    .HasConstraintName("FK__Campaign__PhoneI__37A5467C");

                entity.HasOne(d => d.Website)
                    .WithMany(p => p.Campaigns)
                    .HasForeignKey(d => d.WebsiteId)
                    .HasConstraintName("FK__Campaign__Websit__35BCFE0A");
            });

            modelBuilder.Entity<CustomerField>(entity =>
            {
                entity.ToTable("CustomerField");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.DataType)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.DisplayName).HasMaxLength(200);

                entity.Property(e => e.KeyName)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.HasOne(d => d.Website)
                    .WithMany(p => p.CustomerFields)
                    .HasForeignKey(d => d.WebsiteId)
                    .HasConstraintName("FK__CustomerF__Websi__32E0915F");

            });

            modelBuilder.Entity<EmailAccount>(entity =>
            {
                entity.ToTable("Email_Account");

                entity.Property(e => e.AcocuntId)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.Email)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.Password).IsUnicode(false);

                entity.HasOne(d => d.Acocunt)
                    .WithMany(p => p.EmailAccounts)
                    .HasForeignKey(d => d.AcocuntId)
                    .HasConstraintName("FK__Email_Acc__Acocu__2E1BDC42");
            });

            modelBuilder.Entity<PhoneAccount>(entity =>
            {
                entity.ToTable("Phone_Account");

                entity.Property(e => e.AccountId)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.Phone)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.PhoneAccounts)
                    .HasForeignKey(d => d.AccountId)
                    .HasConstraintName("FK__Phone_Acc__Accou__267ABA7A");
            });

            modelBuilder.Entity<Website>(entity =>
            {
                entity.ToTable("Website");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.Guid)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.Url)
                    .IsUnicode(false)
                    .HasColumnName("URL");

                entity.Property(e => e.AccountId)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.Websites)
                    .HasForeignKey(d => d.AccountId);

                entity.Property(e => e.ShopifyToken)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ShopifyStore)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.HaravanToken)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.IsDeleted).HasColumnType("bit");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
