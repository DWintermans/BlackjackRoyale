﻿using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlackjackCommon.Entities.User
{
    [Table("user")]
    public class User
    {
        [Key]
        [Column("user_id")]
        public int user_id { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("user_name")]
        public string user_name { get; set; }

        [Required]
        [MaxLength(255)]
        [Column("user_passwordhash")]
        public string user_passwordhash { get; set; }

        [Required]
        [MaxLength(255)]
        [Column("user_passwordsalt")]
        public string user_passwordsalt { get; set; }

        [Column("user_is_moderator")]
        public bool user_is_moderator { get; set; }

        [Column("user_balance")]
        public int user_balance { get; set; }

        [Column("user_total_earnings_amt")]
        public int? user_total_earnings_amt { get; set; }

        [Column("user_total_losses_amt")]
        public int? user_total_losses_amt { get; set; }

        [Column("user_total_playtime")]
        public TimeSpan? user_total_playtime { get; set; }

        [Column("user_status", TypeName = "varchar(50)")]
        [DefaultValue(UserStatus.active)]
        public UserStatus user_status { get; set; }

        [Column("user_punishment_till")]
        public DateTime? user_punishment_till { get; set; }
    }

    public enum UserStatus
    {
        active,
        banned,
        timeout
    }
}
