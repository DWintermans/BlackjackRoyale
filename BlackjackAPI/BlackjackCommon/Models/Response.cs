namespace BlackjackCommon.Models
{
	public class Response
	{
		private static readonly Dictionary<string, string> MessageMap = new Dictionary<string, string>
		{
			{ "Default", "An unexpected error occurred." },
			{ "NewPasswordsDontMatch", "New passwords don't match." },
			{ "OldPasswordsDontMatch", "Old passwords don't match." },
			{ "RepeatedPasswords", "New password can't be the same as old password." },
			{ "UserNotFound", "User not found." },
			{ "UsernameAlreadyTaken", "Username already in use." },
			{ "FriendshipExists", "Friendship request already exists between these users." },

			//returns jwt and message
			{ "SuccessfullAccountCreation", "Account created successfully." },

		};

		public bool Success { get; set; }
		public string Code { get; set; }
		public string Message { get; set; }
		public string? JWT { get; set; }

		public Response(string code)
		{
			Success = false;
			Code = code;
			Message = GetMessageForCode(code);
		}

		public Response()
		{
			Success = true;
		}

		public Response(string jwtToken, string code)
		{
			Success = true;
			JWT = jwtToken;
			Code = code;
			Message = GetMessageForCode(code);
		}

		private string GetMessageForCode(string errorCode)
		{
			return MessageMap.TryGetValue(errorCode, out var message) ? message : "An unexpected error occurred.";
		}
	}
}
