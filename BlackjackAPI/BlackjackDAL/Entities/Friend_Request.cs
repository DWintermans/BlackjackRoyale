using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlackjackCommon.Entities.Friend_Request
{
    [Table("friend_request")]
    public class Friend_Request
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

        [Required]
        [Column("friend_datetime")]
        public DateTime friend_datetime { get; set; }
    }

    public enum FriendStatus
    {
        pending,
        accepted,
        rejected
    }
}
