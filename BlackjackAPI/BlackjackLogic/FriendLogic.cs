using BlackjackCommon.Interfaces.Logic;
using BlackjackCommon.Interfaces.Repository;

namespace BlackjackLogic
{
	public class FriendLogic : IFriendLogic
	{
		private readonly IFriendRepository _friendsDAL;

		public FriendLogic(IFriendRepository friendsDAL)
		{
			_friendsDAL = friendsDAL;
		}
	}
}
