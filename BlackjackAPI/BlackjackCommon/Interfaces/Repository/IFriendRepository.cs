namespace BlackjackCommon.Interfaces.Repository
{
	public interface IFriendRepository
	{
		bool FriendshipExists(int user_id, int befriend_user_id);
		void RequestFriendship(int user_id, int befriend_user_id);
		void UpdateFriendStatus(int user_id, int friend_user_id, string status);
	}
}
