using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlackjackCommon.Entities.Friend
{
	public class Friend
	{
		[Key]
		[Column("friend_user_id")]
		public int friend_user_id { get; set; }

		[Key]
		[Column("friend_befriend_user_id")]
		public int friend_befriend_user_id { get; set; }

		[Required]
		[Column("friend_status")]
		public FriendStatus friend_status { get; set; }
	}

	public enum FriendStatus
	{
		pending,
		accepted,
		rejected
	}
}
