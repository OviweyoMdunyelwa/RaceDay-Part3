
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Models;

namespace RaceDay.API.Data
{
    public class RaceDayDbContext : DbContext
    {
        public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options)
            : base(options)
        {
        }

        public DbSet<RaceDayUser> RaceDayUsers { get; set; } = null!;
        public DbSet<Event> Events { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Enrolment> Enrolments { get; set; } = null!;
        public DbSet<Result> Results { get; set; } = null!;
        public DbSet<RaceDay.API.Models.Route> Routes { get; set; } = null!;
        public DbSet<Weather> Weather { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<RaceDayUser>()
                .HasIndex(user => user.Email)
                .IsUnique();

            modelBuilder.Entity<Result>()
                .HasIndex(result => result.EnrolmentID)
                .IsUnique();
        }
    }
}
