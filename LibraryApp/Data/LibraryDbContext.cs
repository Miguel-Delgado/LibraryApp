using LibraryApp.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApp.Data
{
    public class LibraryDbContext : DbContext
    {
        public DbSet<Author> Authors { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Book> Books { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Просто подключаемся к PostgreSQL без UseLegacyTimestampBehavior
            optionsBuilder.UseNpgsql("Host=localhost;Database=LibraryDb;Username=postgres;Password=YOUR_PASSWORD_HERE");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Author>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Country).HasMaxLength(100);
                // ← Важно: указываем тип поля без часового пояса
                entity.Property(e => e.BirthDate).HasColumnType("timestamp without time zone");
            });

            modelBuilder.Entity<Genre>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
            });

            modelBuilder.Entity<Book>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.ISBN).IsRequired().HasMaxLength(20);

                entity.HasOne(e => e.Author)
                      .WithMany(a => a.Books)
                      .HasForeignKey(e => e.AuthorId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Genre)
                      .WithMany(g => g.Books)
                      .HasForeignKey(e => e.GenreId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}