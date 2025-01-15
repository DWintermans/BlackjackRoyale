using BlackjackCommon.ViewModels;

namespace BlackjackCommon.Interfaces.Repository
{
    public interface IFriendRepository
    {
        bool FriendshipIsPending(int user_id, int befriend_user_id);
        bool FriendshipExists(int user_id, int befriend_user_id);
        void RequestFriendship(int user_id, int befriend_user_id);
        void UpdateFriendStatus(int user_id, int friend_user_id, string status);
        List<FriendRequestModel> GetFriendRequests(int user_id);
        List<SearchModel> FindUser(int user_id, string searchTerm);
    }
}
