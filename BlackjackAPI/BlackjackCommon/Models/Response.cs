namespace BlackjackCommon.Models
{
    public class Response<T>
    {
        private static readonly Dictionary<string, string> MessageMap = new Dictionary<string, string>
        {
            { "Default", "An unexpected error occurred." },

            { "EmptyUsername", "Username is required." },
            { "UsernameTooLong", "Username cannot exceed 50 characters." },
            { "UsernameMustContainAlphabetic", "Username must have atleast 1 alphabetic character." },
            { "UsernameFormatInvalid", "Username can only contain letters (including accented letters) and numbers." },

            { "EmptyPassword", "Password is required." },
            { "PasswordTooShort", "Password must be at least 6 characters long." },
            { "PasswordTooLong", "Password cannot exceed 255 characters." },
            { "PasswordFormatInvalid", "Password must contain atleast one number and one special charachter." },

            { "NewPasswordsDontMatch", "New passwords don't match." },
            { "OldPasswordsDontMatch", "Old passwords don't match." },
            { "RepeatedPasswords", "New password can't be the same as old password." },

            { "UserNotFound", "User not found." },
            { "UsernameAlreadyTaken", "Username already in use." },
            { "UsernameAlreadyTakenByUser", "You already have this username." },

            { "NoPendingFriendshipFound", "No valid pending friend request found." },
            { "PendingFriendshipFound", "You already have a pending friendrequest with this user." },
            { "FriendshipExists", "You are already friends with this user." },
            { "InvalidFriendId", "Friend user ID is required." },
            { "FriendIdDoesntExist", "Friend user ID does not exist." },
            { "InvalidFriendStatus", "Status must be either 'accepted' or 'rejected'." },

			//returns jwt and message
			{ "SuccessfullAccountCreation", "Account created successfully." },
            { "SuccessfullLogin", "Logging in." },

            { "Success", "Success" },

            { "NoMessagesFound", "No messages found." },

        };

        public bool Success { get; set; }
        public string Message { get; set; }
        public T? Data { get; set; }

        public Response(string code)
        {
            Success = false;
            Message = GetMessageForCode(code);
        }

        public Response()
        {
            Success = true;
        }

        public Response(T data, string code)
        {
            Success = true;
            Data = data;
            Message = GetMessageForCode(code);
        }

        private string GetMessageForCode(string errorCode)
        {
            return MessageMap.TryGetValue(errorCode, out var message) ? message : "An unexpected error in error-code translation occurred.";
        }
    }
}
