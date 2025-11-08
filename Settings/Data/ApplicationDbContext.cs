using ChatService.Application.Connection.Model;
using ChatService.Application.Person.Model;
using Microsoft.EntityFrameworkCore;
using System;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ChatService.Settings.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Connections> Connections { get; set; }
        public DbSet<Person> Person { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Person configuration
            modelBuilder.Entity<Person>(entity =>
            {
                entity.ToTable("Person");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name)
                      .IsRequired()
                      .HasColumnType("text");
                entity.Property(e => e.Username)
                      .IsRequired()
                      .HasColumnType("text");
                entity.Property(e => e.Password)
                      .IsRequired()
                      .HasColumnType("text");

                entity.HasIndex(e => e.Name);
                entity.HasIndex(e => e.Username);
                entity.HasIndex(e => e.Password);
            });

            // Connections configuration
            modelBuilder.Entity<Connections>(entity =>
            {
                entity.ToTable("Connections");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.SignalrId)
                      .IsRequired()
                      .HasColumnType("text");

                entity.Property(e => e.TimeStamp)
                      .IsRequired()
                      .HasColumnType("timestamp with time zone");

                entity.HasIndex(e => e.SignalrId);
                entity.HasIndex(e => e.TimeStamp);

                entity.HasOne<Person>()
                      .WithMany()
                      .HasForeignKey(e => e.PersonId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
