﻿namespace BlackjackCommon.Interfaces.Repository
{
	public interface IUserRepository
	{
		(int user_id, string user_name, byte[] hashed_pw, byte[] salt) RetrieveLoginInformation(string username);
		(byte[] hashed_pw, byte[] salt) RetrieveSalt_HashInformation(int user_id);
		int CreateAccount(string username, string hashed_password, string salt);
		bool IsUsernameTaken(string username);
		bool UpdatePassword(int user_id, string hashed_password, string salt);
		bool UpdateUsername(int user_id, string user_name);
	}
}