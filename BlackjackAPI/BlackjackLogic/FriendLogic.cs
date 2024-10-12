using BlackjackCommon.Interfaces.Logic;
using BlackjackCommon.Interfaces.Repository;

namespace BlackjackLogic
{
	public class FriendLogic : IFriendLogic
	{
		private readonly IFriendRepository _friendDAL;

		public FriendLogic(IFriendRepository friendDAL)
		{
			_friendDAL = friendDAL;
		}

		public void RequestFriendship(int user_id, int befriend_user_id)
		{
			_friendDAL.RequestFriendship(user_id, befriend_user_id);
		}

		public void UpdateFriendStatus(int user_id, int friend_user_id, string status)
		{
			_friendDAL.UpdateFriendStatus(user_id, friend_user_id, status);
		}

	}
}
