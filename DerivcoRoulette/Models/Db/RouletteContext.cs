using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace DerivcoRoulette.Models.Db
{
    public partial class RouletteContext : DbContext
    {
        public RouletteContext()
        {
        }

        public RouletteContext(DbContextOptions<RouletteContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Bet> Bets { get; set; } = null!;
        public virtual DbSet<Payout> Payouts { get; set; } = null!;
        public virtual DbSet<Spin> Spins { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                string dbPath = Path.Combine(AppContext.BaseDirectory, "test.roulette.sqlite");
                if (!File.Exists(dbPath)) throw new FileNotFoundException($"Could not find {dbPath}");
                optionsBuilder.UseSqlite($"DataSource={dbPath};Cache=Shared");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Bet>(entity =>
            {
                entity.ToTable("bet");

                entity.Property(e => e.BetId)
                    .HasColumnType("integer")
                    .HasColumnName("bet_id");

                entity.Property(e => e.BetOn)
                    .HasColumnType("text")
                    .HasColumnName("bet_on");

                entity.Property(e => e.BetValue)
                    .HasColumnType("integer")
                    .HasColumnName("bet_value");

                entity.Property(e => e.BetWinnings)
                    .HasColumnType("integer")
                    .HasColumnName("bet_winnings");

                entity.Property(e => e.PayoutId)
                    .HasColumnType("integer")
                    .HasColumnName("payout_id");

                entity.Property(e => e.SpinId)
                    .HasColumnType("integer")
                    .HasColumnName("spin_id");

                entity.Property(e => e.TimestampUtc)
                    .HasColumnType("integer")
                    .HasColumnName("timestamp_utc");

                entity.HasOne(d => d.Payout)
                    .WithMany(p => p.Bets)
                    .HasForeignKey(d => d.PayoutId);

                entity.HasOne(d => d.Spin)
                    .WithMany(p => p.Bets)
                    .HasForeignKey(d => d.SpinId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<Payout>(entity =>
            {
                entity.ToTable("payout");

                entity.Property(e => e.PayoutId)
                    .HasColumnType("integer")
                    .HasColumnName("payout_id");

                entity.Property(e => e.TimestampUtc)
                    .HasColumnType("integer")
                    .HasColumnName("timestamp_utc");
            });

            modelBuilder.Entity<Spin>(entity =>
            {
                entity.ToTable("spin");

                entity.Property(e => e.SpinId)
                    .HasColumnType("integer")
                    .HasColumnName("spin_id");

                entity.Property(e => e.SpinResult)
                    .HasColumnType("integer")
                    .HasColumnName("spin_result");

                entity.Property(e => e.TimestampUtc)
                    .HasColumnType("integer")
                    .HasColumnName("timestamp_utc");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
