// <copyright file="ChangeUsername.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace BlackjackAPI.Models.User
{
    using System.ComponentModel.DataAnnotations;

    public class ChangeUsername
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, ErrorMessage = "Username cannot exceed 50 characters")]
        [RegularExpression(@"^[a-zA-Z0-9À-ÖØ-öø-ÿ\s]+$", ErrorMessage = "Username can only contain letters (including accented letters) and numbers")]
        public string Username { get; set; }
    }
}