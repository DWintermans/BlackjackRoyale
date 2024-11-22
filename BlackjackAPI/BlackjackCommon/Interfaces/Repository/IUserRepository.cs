namespace BlackjackCommon.Interfaces.Repository
{
	public interface IUserRepository
	{
		int RetrieveCredits(int user_id);
		void UpdateCredits(int user_id, int credits);
		void UpdateStatistics(int user_id, int gameWins, int gameLosses, int earnings, int losses);
		(int user_id, string user_name, byte[] hashed_pw, byte[] salt) RetrieveLoginInformation(string username);
		(byte[] hashed_pw, byte[] salt) RetrieveSalt_HashInformation(int user_id);
		int CreateAccount(string username, string hashed_password, string salt);
		bool IsUsernameTaken(string username);
		void UpdatePassword(int user_id, string hashed_password, string salt);
		void UpdateUsername(int user_id, string user_name);
		bool IsUsernameTakenByCurrentUser(int user_id, string user_name);
		bool UserIDExists(int user_id);
	}
}
