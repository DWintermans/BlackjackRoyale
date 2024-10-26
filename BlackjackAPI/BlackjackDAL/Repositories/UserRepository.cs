using BlackjackCommon.Entities.User;
using BlackjackCommon.Interfaces.Repository;
using BlackjackCommon.Models;

namespace BlackjackDAL.Repositories
{
	public class UserRepository : IUserRepository
	{
		private readonly DBConnection _DBConnection = new();


		public int RetrieveCredits(int user_id)
		{
			try
			{
				using (var context = new AppDbContext(_DBConnection.ConnectionString()))
				{
					var user = context.User.SingleOrDefault(u => u.user_id == user_id);

					if (user != null)
					{
						return user.user_balance;
					}
					else
					{
						throw new Exception($"User with ID {user_id} not found.");
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"An error occurred: {ex.Message}");
				throw;
			}
		}

		public void UpdateCredits(int user_id, int credits)
		{
			try
			{
				using (var context = new AppDbContext(_DBConnection.ConnectionString()))
				{
					var user = context.User.SingleOrDefault(u => u.user_id == user_id);

					if (user != null)
					{
						user.user_balance = credits;

						context.SaveChanges();
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"An error occurred: {ex.Message}");
				throw;
			}
		}

		public (int user_id, string user_name, byte[] hashed_pw, byte[] salt) RetrieveLoginInformation(string username)
		{
			try
			{
				using (var context = new AppDbContext(_DBConnection.ConnectionString()))
				{
					var user = context.User.SingleOrDefault(u => u.user_name == username);

					if (user != null)
					{
						byte[] hashed_pw = Convert.FromBase64String(user.user_passwordhash);
						byte[] salt = Convert.FromBase64String(user.user_passwordsalt);

						return (user.user_id, user.user_name, hashed_pw, salt);
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"An error occurred: {ex.Message}");
			}

			return (0, null, null, null);
		}

		public (byte[] hashed_pw, byte[] salt) RetrieveSalt_HashInformation(int user_id)
		{
			try
			{
				using (var context = new AppDbContext(_DBConnection.ConnectionString()))
				{
					var user = context.User
									  .Where(u => u.user_id == user_id)
									  .Select(u => new { u.user_passwordhash, u.user_passwordsalt })
									  .FirstOrDefault();

					if (user != null)
					{
						byte[] hashed_pw = Convert.FromBase64String(user.user_passwordhash);
						byte[] salt = Convert.FromBase64String(user.user_passwordsalt);

						return (hashed_pw, salt);
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"An error occurred: {ex.Message}");
			}

			return (null, null);
		}

		public int CreateAccount(string username, string hashed_password, string salt)
		{
			try
			{
				using (var context = new AppDbContext(_DBConnection.ConnectionString()))
				{
					var newUser = new User
					{
						user_name = username,
						user_passwordhash = hashed_password,
						user_passwordsalt = salt,
						user_is_moderator = false,
						user_status = UserStatus.active
					};

					context.User.Add(newUser);

					context.SaveChanges();

					return newUser.user_id;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"An error occurred: {ex.Message}");
				return 0;
			}
		}

		public bool IsUsernameTaken(string username)
		{
			try
			{
				using (var context = new AppDbContext(_DBConnection.ConnectionString()))
				{
					return context.User.Any(u => u.user_name == username);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"An error occurred: {ex.Message}");
				return false;
			}
		}

		public bool IsUsernameTakenByCurrentUser(int user_id, string username)
		{
			try
			{
				using (var context = new AppDbContext(_DBConnection.ConnectionString()))
				{
					return context.User.Any(u => u.user_name == username && u.user_id == user_id);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"An error occurred: {ex.Message}");
				return false;
			}
		}

		public void UpdateUsername(int user_id, string user_name)
		{
			try
			{
				using (var context = new AppDbContext(_DBConnection.ConnectionString()))
				{
					var user = context.User.SingleOrDefault(u => u.user_id == user_id);

					if (user != null)
					{
						user.user_name = user_name;

						context.SaveChanges();
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"An error occurred: {ex.Message}");
				throw;
			}
		}

		public void UpdatePassword(int user_id, string hashed_password, string salt)
		{
			try
			{
				using (var context = new AppDbContext(_DBConnection.ConnectionString()))
				{
					var user = context.User.SingleOrDefault(u => u.user_id == user_id);

					if (user != null)
					{
						user.user_passwordhash = hashed_password;
						user.user_passwordsalt = salt;

						context.SaveChanges();
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"An error occurred: {ex.Message}");
				throw;
			}
		}

	}
}
