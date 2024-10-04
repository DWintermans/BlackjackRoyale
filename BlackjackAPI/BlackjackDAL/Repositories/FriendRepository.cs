using BlackjackCommon.Entities.Account;
using BlackjackCommon.Entities.Friend;
using BlackjackCommon.Interfaces.Repository;
using Google.Protobuf.Collections;
using Org.BouncyCastle.Asn1.Ocsp;

namespace BlackjackDAL.Repositories
{
	public class FriendRepository : IFriendRepository
	{
		private readonly DBConnection _DBConnection = new();

		public void RequestFriendship(int user_id, int befriend_user_id)
		{
			try
			{
				using (var context = new AppDbContext(_DBConnection.ConnectionString()))
				{
					//check if friendship requested (if already pending/accepted/rejected)
					var existingFriendship = context.Friend
						.FirstOrDefault(f =>
							(f.friend_user_id == befriend_user_id && f.friend_befriend_user_id == user_id) ||
							(f.friend_user_id == user_id && f.friend_befriend_user_id == befriend_user_id));

					if (existingFriendship == null)
					{
						var newFriendship = new Friend
						{
							friend_user_id = user_id,
							friend_befriend_user_id = befriend_user_id,
							friend_status = FriendStatus.pending
						};

						context.Friend.Add(newFriendship);
						context.SaveChanges(); 
					}
					else
					{
						throw new InvalidOperationException("Friendship request already exists between these users.");
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"An error occurred: {ex.Message}");
				throw;
			}
		}

		public void UpdateFriendStatus(int user_id, int friend_user_id, string status) 
		{
			try
			{
				using (var context = new AppDbContext(_DBConnection.ConnectionString()))
				{
					var friendship = context.Friend
						.FirstOrDefault(f =>
							(f.friend_user_id == friend_user_id && f.friend_befriend_user_id == user_id));
					
					if (friendship != null)
					{
						friendship.friend_status = Enum.Parse<FriendStatus>(status); 
						context.SaveChanges();
					}
					else
					{
						throw new InvalidOperationException("Friendship does not exist between these users.");
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"An error occurred: {ex.Message}");
				throw;
			}
		}
	}
}
