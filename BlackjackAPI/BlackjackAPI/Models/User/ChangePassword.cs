// <copyright file="ChangePassword.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace BlackjackAPI.Models.User
{
    using System.ComponentModel.DataAnnotations;

    public class ChangePassword
    {
        [Required(ErrorMessage = "Old password is required")]
        [StringLength(255, MinimumLength = 6, ErrorMessage = "Old password must be at least 6 characters long")]
        public string Old_password { get; set; }

        [Required(ErrorMessage = "New password is required")]
        [StringLength(255, MinimumLength = 6, ErrorMessage = "New password must be at least 6 characters long")]
        public string New_password { get; set; }

        [Required(ErrorMessage = "Repeat new password is required")]
        [StringLength(255, MinimumLength = 6, ErrorMessage = "Repeat new password must be at least 6 characters long")]
        public string Repeat_new_password { get; set; }
    }
}