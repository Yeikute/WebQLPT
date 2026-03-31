using Microsoft.EntityFrameworkCore;
using WebQLPT.Models;

namespace WebQLPT.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<PhongTro> PhongTros { get; set; }
        public DbSet<KhachThue> KhachThues { get; set; }
        public DbSet<ChuTro> ChuTros { get; set; }
        public DbSet<DangTin> DangTins { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure DangTin entity
            modelBuilder.Entity<DangTin>(entity =>
            {
                entity.Property(d => d.HinhAnh)
                    .IsRequired(false)
                    .HasColumnType("nvarchar(max)");

                entity.Property(d => d.Gia)
                    .HasPrecision(18, 2);

                entity.HasOne(d => d.PhongTro)
                    .WithMany()
                    .HasForeignKey(d => d.PhongTroId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(d => d.ChuTro)
                    .WithMany()
                    .HasForeignKey(d => d.ChuTroId)
                    .OnDelete(DeleteBehavior.NoAction);
            });
        }
    }


}
