using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Qadaya.Models;

namespace Qadaya.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<LegalCase> Cases => Set<LegalCase>();
        public DbSet<Committee> Committees => Set<Committee>();
        public DbSet<CommitteeMember> CommitteeMembers => Set<CommitteeMember>();
        public DbSet<CaseHistory> CaseHistories => Set<CaseHistory>();
        public DbSet<DisciplinaryRecord> DisciplinaryRecords => Set<DisciplinaryRecord>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<LegalCase>(entity =>
            {
                entity.HasIndex(c => c.CaseNumber).IsUnique();
                entity.HasOne(c => c.Committee)
                    .WithMany(c => c.Cases)
                    .HasForeignKey(c => c.CommitteeId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<CaseHistory>(entity =>
            {
                entity.HasOne(h => h.Case)
                    .WithMany(c => c.HistoryEntries)
                    .HasForeignKey(h => h.CaseId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<CommitteeMember>(entity =>
            {
                entity.HasOne(m => m.Committee)
                    .WithMany(c => c.Members)
                    .HasForeignKey(m => m.CommitteeId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(m => m.User)
                    .WithMany(u => u.CommitteeMemberships)
                    .HasForeignKey(m => m.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<DisciplinaryRecord>(entity =>
            {
                entity.HasOne(d => d.Case)
                    .WithMany(c => c.DisciplinaryRecords)
                    .HasForeignKey(d => d.CaseId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(d => d.Committee)
                    .WithMany()
                    .HasForeignKey(d => d.CommitteeId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}
