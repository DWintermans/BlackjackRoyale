using BlackjackCommon.Entities.User;
using BlackjackCommon.Interfaces.Repository;
using BlackjackCommon.Models;
using MySql.Data.MySqlClient;

namespace BlackjackDAL.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public int RetrieveCredits(int user_id)
        {
            try
            {
                var user = _context.User.SingleOrDefault(u => u.user_id == user_id);

                if (user != null)
                {
                    return user.user_balance;
                }
                else
                {
                    throw new Exception($"User with ID {user_id} not found.");
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
                var user = _context.User.SingleOrDefault(u => u.user_id == user_id);

                if (user != null)
                {
                    user.user_balance = credits;

                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw;
            }
        }

        public void UpdateStatistics(int user_id, int earnings, int losses)
        {
            try
            {
                var user = _context.User.SingleOrDefault(u => u.user_id == user_id);

                if (user != null)
                {
                    user.user_total_earnings_amt += earnings;
                    user.user_total_losses_amt += losses;

                    _context.SaveChanges();
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
                var user = _context.User.SingleOrDefault(u => u.user_name == username);

                if (user != null)
                {
                    byte[] hashed_pw = Convert.FromBase64String(user.user_passwordhash);
                    byte[] salt = Convert.FromBase64String(user.user_passwordsalt);

                    return (user.user_id, user.user_name, hashed_pw, salt);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                LogToFile(ex, username);
            }

            return (0, null, null, null);
        }

        private void LogToFile(Exception ex, string username)
        {
            string logFilePath = "app-log.txt";
            string logMessage = $"{DateTime.UtcNow}: Error for user {username} - {ex.Message}{Environment.NewLine}{ex.StackTrace}{Environment.NewLine}";
            File.AppendAllText(logFilePath, logMessage);
        }

        public (byte[] hashed_pw, byte[] salt) RetrieveSalt_HashInformation(int user_id)
        {
            try
            {
                var user = _context.User
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
                var newUser = new User
                {
                    user_name = username,
                    user_passwordhash = hashed_password,
                    user_passwordsalt = salt,
                    user_is_moderator = false,
                    user_status = UserStatus.active,
                    user_balance = 100,
                    user_total_earnings_amt = 0,
                    user_total_losses_amt = 0,
                };

                _context.User.Add(newUser);
                _context.SaveChanges();

                return newUser.user_id;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                LogToFile(ex, username);
                return 0;
            }
        }

        public bool IsUsernameTaken(string username)
        {
            try
            {
                return _context.User.Any(u => u.user_name == username);
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
                return _context.User.Any(u => u.user_name == username && u.user_id == user_id);
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
                var user = _context.User.SingleOrDefault(u => u.user_id == user_id);

                if (user != null)
                {
                    user.user_name = user_name;

                    _context.SaveChanges();
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
                var user = _context.User.SingleOrDefault(u => u.user_id == user_id);

                if (user != null)
                {
                    user.user_passwordhash = hashed_password;
                    user.user_passwordsalt = salt;

                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw;
            }
        }

        public bool UserIDExists(int user_id)
        {
            try
            {
                return _context.User.Any(u => u.user_id == user_id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return false;
            }
        }

    }
}
