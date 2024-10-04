namespace BlackjackCommon.Interfaces.Logic
{
	public interface IFriendLogic
	{
		void RequestFriendship(int user_id, int befriend_user_id);
		void UpdateFriendStatus(int user_id, int friend_user_id, string status);
	}
}
