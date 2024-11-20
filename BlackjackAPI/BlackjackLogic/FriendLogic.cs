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

		public FriendLogic(IFriendRepository friendDAL)
		{
			_friendDAL = friendDAL;
		}

		public Response RequestFriendship(int	 user_id, int befriend_user_id)
		{
			if (befriend_user_id < 0)
			{
				return new Response("InvalidFriendId");
			}

			if (_friendDAL.FriendshipExists(user_id, befriend_user_id))
			{
				return new Response("FriendshipExists");
			}

			_friendDAL.RequestFriendship(user_id, befriend_user_id);
			return new Response();
		}

		public Response UpdateFriendStatus(int user_id, int friend_user_id, string status)
		{
			if (friend_user_id < 0) 
			{
				return new Response("InvalidFriendId");
			}

			var valStatusResponse = ValidateStatus(status);
			if (valStatusResponse != null)
			{
				return valStatusResponse;
			}

			_friendDAL.UpdateFriendStatus(user_id, friend_user_id, status);
			return new Response();
		}

		private static Response ValidateStatus(string status)
		{
			if (status.ToLower() != "accepted" && status.ToLower() != "rejected")
			{
				return new Response("InvalidFriendStatus");
			}

			return null;
		}
	}
}
