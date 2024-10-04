namespace BlackjackCommon.Interfaces.Logic
{
	public interface IAccountLogic
	{
		AccountResult CreateAccount(string username, string password);
		int ValidateUser(string username, string password);
		AccountResult ChangePassword(int user_id, string old_password, string new_password, string repeat_new_password);
		AccountResult ChangeUsername(int user_id, string user_name);
		string CreateJWT(int user_id, string username);
		int GetUserIDFromJWT(string jwt_token);
	}

	public class AccountResult
	{
		public required bool Success { get; set; }
		public string? Message { get; set; }
		public string? JWT { get; set; }
	}
}
