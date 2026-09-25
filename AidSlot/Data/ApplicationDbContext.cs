using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AidSlot.Models;

namespace AidSlot.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Campaign> Campaigns { get; set; }
        public DbSet<Organization> Organizations => Set<Organization>();
        public DbSet<Recipient> Recipients => Set<Recipient>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUser>()
                .HasOne(user => user.Organization)
                .WithMany()
                .HasForeignKey(user => user.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Campaign>()
                .HasOne(campaign => campaign.Organization)
                .WithMany()
                .HasForeignKey(campaign => campaign.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Recipient>(entity =>
            {
                // A recipient may participate in multiple campaigns, but only once per campaign.
                entity.HasIndex(r => new { r.CampaignId, r.DocumentNumber }).IsUnique();

                entity.Property(r => r.FullName).HasMaxLength(200).IsRequired();
                entity.Property(r => r.SpouseFullName).HasMaxLength(200);
                entity.Property(r => r.ResidentialArea).HasMaxLength(200).IsRequired();
                entity.Property(r => r.HeadOfHouseholdGender).HasMaxLength(20).IsRequired();
                entity.Property(r => r.PhoneNumber).HasMaxLength(30).IsRequired();
                entity.Property(r => r.Nationality).HasMaxLength(100).IsRequired();
                entity.Property(r => r.DocumentType).HasMaxLength(50).IsRequired();
                entity.Property(r => r.DocumentNumber).HasMaxLength(50).IsRequired();

                entity.HasOne(r => r.Campaign)
                    .WithMany(c => c.Recipients)
                    .HasForeignKey(r => r.CampaignId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
