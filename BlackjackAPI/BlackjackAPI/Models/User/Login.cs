// <copyright file="Login.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace BlackjackAPI.Models.User
{
    using System.ComponentModel.DataAnnotations;

    public class Login
    {
        [Required(ErrorMessage = "Username is required")]
        required public string Username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        required public string Password { get; set; }
    }
}
