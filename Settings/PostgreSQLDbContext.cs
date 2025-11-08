using Microsoft.EntityFrameworkCore;
using ChatService.Models;

namespace ChatService.Settings
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Id).HasColumnName("id").UseIdentityAlwaysColumn();
                entity.Property(u => u.Fullname).IsRequired().HasColumnName("fullname");
                entity.Property(u => u.Email).IsRequired().HasColumnName("email");
                entity.Property(u => u.PasswordHash).IsRequired().HasColumnName("passwordhash");
                entity.Property(u => u.ProfileImageIndex).HasColumnName("profileimageindex");
                entity.Property(u => u.IsGoogleAccount).HasColumnName("isgoogleaccount").HasDefaultValue(false);
                entity.Property(u => u.EncryptedOpenAiToken).HasColumnName("encryptedopenaitoken").IsRequired(false);
                entity.Property(u => u.Role).HasColumnName("role").IsRequired().HasDefaultValue("User");
            });
        }
    }
}
