using ChatApp.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ChatApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<ChatRoom> ChatRooms { get; set; }
        

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure ChatMessage
            modelBuilder.Entity<ChatMessage>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Content).IsRequired().HasMaxLength(500);
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.UserName).IsRequired().HasMaxLength(256);
                entity.Property(e => e.RoomName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.IsDeleted).HasDefaultValue(false);

                // Index for better performance
                entity.HasIndex(e => new { e.RoomName, e.CreatedAt });
                entity.HasIndex(e => e.UserId);
            });

            // Configure ChatRoom
            modelBuilder.Entity<ChatRoom>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(256);
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.IsPrivate).HasDefaultValue(false);

                // Store Members as JSON
                entity.Property(e => e.Members)
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                        v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null) ?? new List<string>()
                    );

                // Unique constraint for room name
                entity.HasIndex(e => e.Name).IsUnique();
            });

            // Seed default data
            modelBuilder.Entity<ChatRoom>().HasData(
                new ChatRoom
                {
                    Id = 1,
                    Name = "General",
                    Description = "Phòng chat chung cho tất cả mọi người",
                    CreatedBy = "System",
                    CreatedAt = DateTime.Now,
                    IsPrivate = false,
                    Members = new List<string>()
                }
            );
        }
    }
}
