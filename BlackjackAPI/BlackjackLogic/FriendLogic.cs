using BlackjackCommon.Interfaces.Logic;
using BlackjackCommon.Interfaces.Repository;
using BlackjackCommon.Models;
using System.Reflection;
using System.Text.RegularExpressions;

namespace BlackjackLogic
{
	public class FriendLogic : IFriendLogic
	{
		private readonly IFriendRepository _friendDAL;
		private readonly IUserRepository _userDAL;

		public FriendLogic(IFriendRepository friendDAL, IUserRepository userDAL)
		{
			_friendDAL = friendDAL;
			_userDAL = userDAL;
		}

		public Response<string> RequestFriendship(int user_id, int befriend_user_id)
		{
			if (befriend_user_id < 0)
			{
				return new Response<string>("InvalidFriendId");
			}

			if (!_userDAL.UserIDExists(befriend_user_id))
			{
				return new Response<string>("FriendIdDoesntExist");
			}

			if (_friendDAL.FriendshipExists(user_id, befriend_user_id))
			{
				return new Response<string>("FriendshipExists");
			}

			if (_friendDAL.FriendshipIsPending(user_id, befriend_user_id))
			{
				return new Response<string>("PendingFriendshipFound");
			}

			_friendDAL.RequestFriendship(user_id, befriend_user_id);
			return new Response<string>();
		}

		public Response<string> UpdateFriendStatus(int user_id, int friend_user_id, string status)
		{
			if (friend_user_id < 0) 
			{
				return new Response<string>("InvalidFriendId");
			}

			var valStatusResponse = ValidateStatus(status);
			if (valStatusResponse != null)
			{
				return valStatusResponse;
			}

			if (!_friendDAL.FriendshipIsPending(user_id, friend_user_id))
			{
				return new Response<string>("NoPendingFriendshipFound");
			}

			_friendDAL.UpdateFriendStatus(user_id, friend_user_id, status);
			return new Response<string>();
		}

		private static Response<string> ValidateStatus(string status)
		{
			if (status.ToLower() != "accepted" && status.ToLower() != "rejected")
			{
				return new Response<string>("InvalidFriendStatus");
			}

			return null;
		}
	}
}
