using BlackjackCommon.Entities.Friend;
using BlackjackCommon.Entities.Friend_Request;
using BlackjackCommon.Entities.Message;
using BlackjackCommon.Entities.User;
using BlackjackCommon.Entities.History;
using Microsoft.EntityFrameworkCore;

namespace BlackjackDAL
{
    public class AppDbContext : DbContext
    {
        private readonly string? _connectionString;

        //for testing
        public AppDbContext(DbContextOptions<AppDbContext> options)
           : base(options)
        {
        }

        public AppDbContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured && !string.IsNullOrEmpty(_connectionString))
            {
                if (_connectionString.Contains("localhost", StringComparison.OrdinalIgnoreCase))
                {
                    optionsBuilder.UseMySql(_connectionString, ServerVersion.AutoDetect(_connectionString));
                }
                else
                {
                    optionsBuilder.UseSqlServer(_connectionString);
                }
            }
        }

        public DbSet<User> User { get; set; }
        public DbSet<Friend> Friend { get; set; }
        public DbSet<Friend_Request> Friend_Request { get; set; }
        public DbSet<Message> Message { get; set; }
        public DbSet<History> History { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //set moderator default to false
            modelBuilder.Entity<User>()
                .Property(u => u.user_is_moderator)
                .HasDefaultValue(false);

            //set default user status to active
            modelBuilder.Entity<User>()
                .Property(u => u.user_status)
                .HasConversion<string>()
                .HasDefaultValue(UserStatus.active);

            //composite key
            modelBuilder.Entity<Friend>()
                .HasKey(f => new { f.friend_user_id, f.friend_befriend_user_id });

            //composite key
            modelBuilder.Entity<Friend_Request>()
                .HasKey(f => new { f.friend_user_id, f.friend_befriend_user_id });

            //string conversion for enum
            modelBuilder.Entity<Friend_Request>()
                .Property(f => f.friend_status)
                .HasConversion<string>();

            //string conversion for enum

            modelBuilder.Entity<History>()
                .Property(f => f.history_action)
                .HasConversion<string>();

            //string conversion for enum

            modelBuilder.Entity<History>()
                .Property(f => f.history_result)
                .HasConversion<string>();

            base.OnModelCreating(modelBuilder);
        }

    }
}
