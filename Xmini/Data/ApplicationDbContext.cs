using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Xmini.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        #region DbSets
        // DbSets für die Entitäten
        public DbSet<Like> Likes { get; set; } = default!;
        public DbSet<Tweet> Tweets { get; set; } = default!;
        public DbSet<Followers> Followers { get; set; } = default!;
        #endregion
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // optionale Fluent-API-Konfigurationen, z. B. Länge, Required, DefaultValue
            // builder.Entity<ApplicationUser>().Property(u => u.Vorname).HasMaxLength(100);

            #region Tweet
            // Konfiguration der Beziehung zwischen ApplicationUser und Tweet
            builder.Entity<Tweet>()
                .HasOne(t => t.ApplicationUser)
                .WithMany(u => u.Tweets)
                .HasForeignKey(t => t.ApplicationUserId);
            #endregion
            #region Like
            // Konfiguration der Beziehung zwischen ApplicationUser, Tweet und Like
            builder.Entity<Like>()
                .HasOne(l => l.ApplicationUser)
                .WithMany(u => u.Likes)
                .HasForeignKey(l => l.ApplicationUserId);
            builder.Entity<Like>()
                .HasOne(l => l.Tweet)
                .WithMany(t => t.Likes)
                .HasForeignKey(l => l.TweetId);
            #endregion
            #region Followers
            // Konfiguration der Beziehung zwischen ApplicationUser und Followers
            builder.Entity<Followers>()
                .HasOne(f => f.FollowerUser)
                .WithMany(u => u.Followers)
                .HasForeignKey(f => f.FollowerUserId);
            // Konfiguration der Beziehung zwischen ApplicationUser und Following
            builder.Entity<Followers>()
                .HasOne(f => f.FollowsUser)
                .WithMany(u => u.Following)
                .HasForeignKey(f => f.FollowsUserId);
            #endregion
        }
    }
}
