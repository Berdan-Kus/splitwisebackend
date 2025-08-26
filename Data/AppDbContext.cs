using Microsoft.EntityFrameworkCore;
using SplitwiseAPI.Models;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace SplitwiseAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<UserGroup> UserGroups { get; set; }
        public DbSet<UserExpense> UserExpenses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User Entity Configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Phone).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Password).IsRequired().HasMaxLength(255);

                // Unique constraint for phone number
                entity.HasIndex(e => e.Phone).IsUnique();
            });

            // Group Entity Configuration
            modelBuilder.Entity<Group>(entity =>
            {
                entity.HasKey(e => e.GroupId);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            });

            // Expense Entity Configuration
            modelBuilder.Entity<Expense>(entity =>
            {
                entity.HasKey(e => e.ExpenseId);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Amount).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(e => e.Password).IsRequired().HasMaxLength(255);

                // Foreign key relationship with Group
                entity.HasOne(e => e.Group)
                      .WithMany(g => g.Expenses)
                      .HasForeignKey(e => e.GroupId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // UserGroup Entity Configuration (Junction Table)
            modelBuilder.Entity<UserGroup>(entity =>
            {
                entity.HasKey(e => e.UserGroupId);

                // Foreign key relationships
                entity.HasOne(ug => ug.User)
                      .WithMany(u => u.UserGroups)
                      .HasForeignKey(ug => ug.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ug => ug.Group)
                      .WithMany(g => g.UserGroups)
                      .HasForeignKey(ug => ug.GroupId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Composite unique constraint to prevent duplicate user-group pairs
                entity.HasIndex(e => new { e.UserId, e.GroupId }).IsUnique();
            });

            // UserExpense Entity Configuration
            modelBuilder.Entity<UserExpense>(entity =>
            {
                entity.HasKey(e => e.UserExpenseId);
                entity.Property(e => e.Amount).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(e => e.Type).IsRequired();

                // Foreign key relationships
                entity.HasOne(ue => ue.User)
                      .WithMany(u => u.UserExpenses)
                      .HasForeignKey(ue => ue.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ue => ue.Expense)
                      .WithMany(e => e.UserExpenses)
                      .HasForeignKey(ue => ue.ExpenseId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Composite unique constraint to prevent duplicate user-expense-type combinations
                entity.HasIndex(e => new { e.UserId, e.ExpenseId, e.Type }).IsUnique();
            });

            // Enum Configuration
            modelBuilder.Entity<UserExpense>()
                       .Property(e => e.Type)
                       .HasConversion<string>();
        }
    }
}