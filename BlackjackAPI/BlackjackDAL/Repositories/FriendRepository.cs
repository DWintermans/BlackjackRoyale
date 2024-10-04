using BlackjackCommon.Interfaces.Repository;

namespace BlackjackDAL.Repositories
{
	public class FriendRepository : IFriendRepository
	{
		private readonly DBConnection _DBConnection = new();

	}
}
