using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using web_api_auth.Models;

namespace web_api_auth.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Game> Games { get; set; }
        public DbSet<Turn> Turns { get; set; }
        public DbSet<Roll> Rolls { get; set; }
        public DbSet<ApplicationUser> Players { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Map entities to tables  
            modelBuilder.Entity<Game>()
                .ToTable("Games");
            modelBuilder.Entity<Turn>()
                .ToTable("Turns");
            modelBuilder.Entity<Roll>()
                .ToTable("Rolls");

            // Configure Primary Keys  
            modelBuilder.Entity<Game>()
                .HasKey(g => g.GameId)
                .HasName("PK_Games");
            modelBuilder.Entity<Turn>()
                .HasKey(t => t.TurnId)
                .HasName("PK_Turns");
            modelBuilder.Entity<Roll>()
                .HasKey(t => t.RollId)
                .HasName("PK_Rolls");

            // Configure indexes

            // Games column
            modelBuilder.Entity<Game>()
                .Property(game => game.GameId)
                .HasColumnType("int")
                .UseMySqlIdentityColumn()
                .IsRequired();

            // Turns column
            modelBuilder.Entity<Turn>()
                .Property(t => t.TurnId)
                .HasColumnType("int")
                .UseMySqlIdentityColumn()
                .IsRequired();
            modelBuilder.Entity<Turn>()
                .Property(t => t.Score)
                .HasColumnType("nvarchar(15)")
                .HasDefaultValue(null);
            modelBuilder.Entity<Turn>()
                .Property(t => t.GameId)
                .HasColumnType("int")
                .IsRequired();

            // Rolls column
            modelBuilder.Entity<Roll>()
                .Property(t => t.RollId)
                .HasColumnType("int")
                .UseMySqlIdentityColumn()
                .IsRequired();
            modelBuilder.Entity<Roll>()
                .Property(t => t.DiceOne)
                .HasColumnType("int")
                .IsRequired();
            modelBuilder.Entity<Roll>()
                .Property(t => t.DiceTwo)
                .HasColumnType("int")
                .IsRequired();
            modelBuilder.Entity<Roll>()
                .Property(t => t.DiceThree)
                .HasColumnType("int")
                .IsRequired();
            modelBuilder.Entity<Roll>()
                .Property(t => t.DiceFour)
                .HasColumnType("int")
                .IsRequired();
            modelBuilder.Entity<Roll>()
                .Property(t => t.DiceFive)
                .HasColumnType("int")
                .IsRequired();
            modelBuilder.Entity<Roll>()
                .Property(t => t.DiceToKeep)
                .HasColumnType("nvarchar(5)")
                .IsRequired();
            modelBuilder.Entity<Roll>()
                .Property(t => t.TurnId)
                .HasColumnType("int")
                .IsRequired();

            // Relationships
            modelBuilder.Entity<Game>()
                .HasMany(g => g.Turns)
                .WithOne(t => t.Game);

            modelBuilder.Entity<Turn>()
                .HasOne(t => t.Game)
                .WithMany(g => g.Turns)
                .HasForeignKey(t => t.GameId)
                .HasConstraintName("FK_Turn_Game");
            modelBuilder.Entity<Turn>()
                .HasMany(t => t.Rolls)
                .WithOne(r => r.Turn);

            modelBuilder.Entity<Roll>()
                .HasOne(r => r.Turn)
                .WithMany(t => t.Rolls)
                .HasForeignKey(t => t.TurnId)
                .HasConstraintName("FK_Roll_Turn");

            modelBuilder.Entity<Game>()
                .HasMany(g => g.Players)
                .WithMany(p => p.Games)
                .UsingEntity("ApplicationUserGame");
        }
    }
}

