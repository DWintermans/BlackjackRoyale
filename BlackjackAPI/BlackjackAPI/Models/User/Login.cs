﻿using System.ComponentModel.DataAnnotations;

namespace BlackjackAPI.Models.User
{
	public class Login
	{
		[Required(ErrorMessage = "Username is required")]
		public string username { get; set; }


		[Required(ErrorMessage = "Password is required")]
		public string password { get; set; }
	}
}