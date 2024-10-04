using BlackjackCommon.Entities.Account;
using BlackjackCommon.Interfaces.Repository;
using BlackjackDAL;
using MySql.Data.MySqlClient;

namespace BlackjackDAL.Repositories
{
	public class AccountRepository : IAccountRepository
	{
		private readonly DBConnection _DBConnection = new();

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

		public bool UpdateUsername(int user_id, string user_name)
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

						return true; 
					}

					return false;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"An error occurred: {ex.Message}");
				return false;
			}
		}

		public bool UpdatePassword(int user_id, string hashed_password, string salt)
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

						return true; 
					}

					return false; 
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"An error occurred: {ex.Message}");
				return false;
			}
		}


	}
}
