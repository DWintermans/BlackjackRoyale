namespace BlackjackAPI.Models.Account
{
	public class ChangePassword
	{
		public string old_password { get; set; }
		public string new_password { get; set; }
		public string repeat_new_password { get; set; }
	}
}