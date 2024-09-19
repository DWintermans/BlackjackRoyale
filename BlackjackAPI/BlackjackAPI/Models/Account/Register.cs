using System.ComponentModel.DataAnnotations;

namespace BlackjackAPI.Models.Account
{
    public class Register
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, ErrorMessage = "Username cannot exceed 50 characters")]
		[RegularExpression(@"^[a-zA-Z0-9À-ÖØ-öø-ÿ]+$", ErrorMessage = "Username can only contain letters (including accented letters) and numbers")]
		public string username { get; set; }


        [Required(ErrorMessage = "Password is required")]
        [StringLength(255, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long")]
        public string password { get; set; }
    }
}
