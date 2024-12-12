using BlackjackCommon.Entities.Friend;
using BlackjackCommon.Entities.Friend_Request;
using BlackjackCommon.Interfaces.Repository;
using BlackjackCommon.ViewModels;

namespace BlackjackDAL.Repositories
{
	public class FriendRepository : IFriendRepository
	{
		private readonly AppDbContext _context;

		public FriendRepository(AppDbContext context)
		{
			_context = context;
		}

		public List<FriendRequestModel> GetFriendRequests(int user_id) 
		{
			try
			{	
				//get all relevant requests for the userid as requester or receiver
				var friendRequests = _context.Friend_Request
				   .Where(f => f.friend_user_id == user_id || f.friend_befriend_user_id == user_id)
					.AsEnumerable()
					.GroupBy(f => new
					{
						MinUserId = Math.Min(f.friend_user_id, f.friend_befriend_user_id),
						MaxUserId = Math.Max(f.friend_user_id, f.friend_befriend_user_id)
					})
					.Select(g => g.OrderByDescending(f => f.friend_datetime).FirstOrDefault())
					.Where(f => f.friend_status == FriendStatus.pending)
					.ToList();

				//build model
				var result = friendRequests.Select(f => new FriendRequestModel
				{
					user_id = f.friend_user_id == user_id ? f.friend_befriend_user_id : f.friend_user_id,
					user_name = _context.User
						.Where(u => u.user_id == (f.friend_user_id == user_id ? f.friend_befriend_user_id : f.friend_user_id))
						.Select(u => u.user_name)
						.FirstOrDefault(),
					can_answer = f.friend_befriend_user_id == user_id //if is receiver > true, if requested the friendship set to false.
				}).ToList();

				return result;	
			}
			catch (Exception ex)
			{
				Console.WriteLine($"An error occurred: {ex.Message}");
				throw;
			}
		}

		public bool FriendshipIsPending(int user_id, int befriend_user_id) 
		{
			try
			{
				//check if friendship is pending
				var friendRequest = _context.Friend_Request
				.Where(f =>
					(f.friend_user_id == user_id && f.friend_befriend_user_id == befriend_user_id) ||
					(f.friend_user_id == befriend_user_id && f.friend_befriend_user_id == user_id))
				.OrderByDescending(f => f.friend_datetime)
				.FirstOrDefault();

				return friendRequest != null && friendRequest.friend_status == FriendStatus.pending;		
			}
			catch (Exception ex)
			{
				Console.WriteLine($"An error occurred: {ex.Message}");
				throw;
			}
		}

		public bool FriendshipExists(int user_id, int befriend_user_id)
		{
			try
			{		
				//check if friendship already exists
				var existingFriendship = _context.Friend
					.FirstOrDefault(f =>
						(f.friend_user_id == befriend_user_id && f.friend_befriend_user_id == user_id) ||
						(f.friend_user_id == user_id && f.friend_befriend_user_id == befriend_user_id));

				return existingFriendship != null;	
			}
			catch (Exception ex)
			{
				Console.WriteLine($"An error occurred: {ex.Message}");
				throw;
			}
		}

		public void RequestFriendship(int user_id, int befriend_user_id)
		{
			try
			{	
				var newFriendship = new Friend_Request
				{
					friend_user_id = user_id,
					friend_befriend_user_id = befriend_user_id,
					friend_status = FriendStatus.pending,
					friend_datetime = DateTime.Now
				};

				_context.Friend_Request.Add(newFriendship);
				_context.SaveChanges();
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
				//log action
				var newFriendship = new Friend_Request
				{
					friend_user_id = user_id,
					friend_befriend_user_id = friend_user_id,
					friend_status = Enum.Parse<FriendStatus>(status),
					friend_datetime = DateTime.Now
				};

				_context.Friend_Request.Add(newFriendship);

				if (status == "accepted")
				{
					var newFriend = new Friend
					{
						friend_user_id = user_id,
						friend_befriend_user_id = friend_user_id
					};

					_context.Friend.Add(newFriend);
				}

				_context.SaveChanges();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"An error occurred: {ex.Message}");
				throw;
			}
		}
	}
}
