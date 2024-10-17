using System.ComponentModel.DataAnnotations;

namespace BlackjackAPI.Models.Friend
{
	public class StatusModel
	{
		[Required(ErrorMessage = "Friend user ID is required")]
		public int friend_user_id { get; set; }

		[Required(ErrorMessage = "Status is required")]
		[RegularExpression("^(accepted|rejected)$", ErrorMessage = "Status must be either 'accepted' or 'rejected'")]
		public string status { get; set; }
	}
}
