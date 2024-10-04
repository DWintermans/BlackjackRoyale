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

		//public (int user_id, string user_name, byte[] hashed_pw, byte[] salt) RetrieveLoginInformation(string username)
		//{
		//	try
		//	{
		//		using (MySqlConnection conn = new MySqlConnection(_DBConnection.ConnectionString()))
		//		{
		//			conn.Open();

		//			using (MySqlCommand cmd = new MySqlCommand())
		//			{
		//				cmd.CommandText = "SELECT user_id, user_name, user_passwordhash, user_passwordsalt FROM user WHERE user_name = @user_name";
		//				cmd.Parameters.AddWithValue("@user_name", username);
		//				cmd.Connection = conn;

		//				using (MySqlDataReader reader = cmd.ExecuteReader())
		//				{
		//					if (reader.HasRows && reader.Read())
		//					{
		//						int user_id = Convert.ToInt32(reader["user_id"]);
		//						string user_name = reader["user_name"].ToString();
		//						string hashed_pwString = reader["user_passwordhash"].ToString();
		//						string saltString = reader["user_passwordsalt"].ToString();

		//						byte[] hashed_pw = Convert.FromBase64String(hashed_pwString);
		//						byte[] salt = Convert.FromBase64String(saltString);

		//						return (user_id, user_name, hashed_pw, salt);
		//					}
		//				}
		//			}
		//		}
		//	}
		//	catch (Exception ex)
		//	{
		//		Console.WriteLine($"An error occurred: {ex.Message}");
		//	}

		//	return (0, null, null, null);
		//}

		public (byte[] hashed_pw, byte[] salt) RetrieveSalt_HashInformation(int user_id)
		{
			try
			{
				using (MySqlConnection conn = new MySqlConnection(_DBConnection.ConnectionString()))
				{
					conn.Open();

					using (MySqlCommand cmd = new MySqlCommand())
					{
						cmd.CommandText = "SELECT user_passwordhash, user_passwordsalt FROM user WHERE user_id = @user_id";
						cmd.Parameters.AddWithValue("@user_id", user_id);
						cmd.Connection = conn;

						using (MySqlDataReader reader = cmd.ExecuteReader())
						{
							if (reader.HasRows && reader.Read())
							{
								string hashed_pwString = reader["user_passwordhash"].ToString();
								string saltString = reader["user_passwordsalt"].ToString();

								byte[] hashed_pw = Convert.FromBase64String(hashed_pwString);
								byte[] salt = Convert.FromBase64String(saltString);

								return (hashed_pw, salt);
							}
						}
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
				using (MySqlConnection conn = new MySqlConnection(_DBConnection.ConnectionString()))
				{
					conn.Open();

					MySqlCommand cmd = new();

					cmd.CommandText = "INSERT INTO user (user_name, user_passwordhash, user_passwordsalt) VALUES (@user_name, @user_passwordhash, @user_passwordsalt)";
					cmd.Parameters.AddWithValue("@user_name", username);
					cmd.Parameters.AddWithValue("@user_passwordhash", hashed_password);
					cmd.Parameters.AddWithValue("@user_passwordsalt", salt);
					cmd.Connection = conn;

					int rowsAffected = cmd.ExecuteNonQuery();

					//try to get user_id for jwt
					if (rowsAffected > 0)
					{
						cmd.CommandText = "SELECT LAST_INSERT_ID()";
						object user_id = cmd.ExecuteScalar();

						//check user_id
						if (user_id != null)
						{
							return Convert.ToInt32(user_id);
						}
						else
						{
							Console.WriteLine("Failed to retrieve user_id after insertion.");
							return 0;
						}
					}
					else
					{
						Console.WriteLine("Failed to insert user into the database.");
						return 0;
					}
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
			using (MySqlConnection conn = new MySqlConnection(_DBConnection.ConnectionString()))
			{
				conn.Open();

				using (MySqlCommand cmd = new MySqlCommand("SELECT EXISTS (SELECT 1 FROM user WHERE user_name = @user_name)", conn))
				{
					cmd.Parameters.AddWithValue("@user_name", username);
					return Convert.ToBoolean(cmd.ExecuteScalar());
				}
			}
		}

		public string GetUserName(int user_id)
		{
			string user_name = null;

			try
			{
				using (MySqlConnection conn = new MySqlConnection(_DBConnection.ConnectionString()))
				{
					conn.Open();

					MySqlCommand cmd = new MySqlCommand();
					cmd.CommandText = @"
                        SELECT user_name FROM user WHERE user_id = @user_id";

					cmd.Parameters.AddWithValue("@user_id", user_id);
					cmd.Connection = conn;

					using (MySqlDataReader reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							user_name = reader["user_name"].ToString();
							break;
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"An error occurred: {ex.Message}");
			}

			return user_name;
		}

		public bool UpdateUsername(int user_id, string user_name)
		{
			try
			{
				using (MySqlConnection conn = new MySqlConnection(_DBConnection.ConnectionString()))
				{
					conn.Open();

					MySqlCommand cmd = new MySqlCommand();
					cmd.Connection = conn;

					cmd.CommandText = @"
                        UPDATE user 
                        SET user_name = @user_name 
                        WHERE user_id = @user_id";

					cmd.Parameters.AddWithValue("@user_name", user_name);
					cmd.Parameters.AddWithValue("@user_id", user_id);

					int rowsAffected = cmd.ExecuteNonQuery();
					return rowsAffected > 0;
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
				using (MySqlConnection conn = new MySqlConnection(_DBConnection.ConnectionString()))
				{
					conn.Open();

					MySqlCommand cmd = new MySqlCommand();
					cmd.Connection = conn;

					cmd.CommandText = @"
                        UPDATE user 
                        SET user_passwordhash = @user_passwordhash, user_passwordsalt = @user_passwordsalt
                        WHERE user_id = @user_id";

					cmd.Parameters.AddWithValue("@user_id", user_id);
					cmd.Parameters.AddWithValue("@user_passwordhash", hashed_password);
					cmd.Parameters.AddWithValue("@user_passwordsalt", salt);

					int rowsAffected = cmd.ExecuteNonQuery();
					return rowsAffected > 0;
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
