using BlackjackCommon.Models;

namespace BlackjackCommon.Interfaces.Logic
{
	public interface IFriendLogic
	{
		Response RequestFriendship(int user_id, int befriend_user_id);
		Response UpdateFriendStatus(int user_id, int friend_user_id, string status);
	}
}
