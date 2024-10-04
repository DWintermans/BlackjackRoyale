using BlackjackCommon.Entities.Account;
using Microsoft.EntityFrameworkCore;

namespace BlackjackDAL
{
	public class AppDbContext : DbContext
	{
		private readonly string _connectionString;

		public AppDbContext(string connectionString)
		{
			_connectionString = connectionString;
		}

		public DbSet<User> User { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseMySql(_connectionString, ServerVersion.AutoDetect(_connectionString));
		}

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

			base.OnModelCreating(modelBuilder);
		}

	}
}
