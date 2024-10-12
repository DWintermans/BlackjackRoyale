using System.ComponentModel.DataAnnotations;

namespace BlackjackAPI.Models.User
{
	public class ChangePassword
	{
		[Required(ErrorMessage = "Old password is required")]
		[StringLength(255, MinimumLength = 6, ErrorMessage = "Old password must be at least 6 characters long")]
		public string old_password { get; set; }

		[Required(ErrorMessage = "New password is required")]
		[StringLength(255, MinimumLength = 6, ErrorMessage = "New password must be at least 6 characters long")]
		public string new_password { get; set; }

		[Required(ErrorMessage = "Repeat new password is required")]
		[StringLength(255, MinimumLength = 6, ErrorMessage = "Repeat new password must be at least 6 characters long")]
		public string repeat_new_password { get; set; }
	}
}