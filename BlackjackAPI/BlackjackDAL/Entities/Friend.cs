using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlackjackCommon.Entities.Friend
{
	[Table("friend")]

	public class Friend
    {
        [Key]
        [Column("friend_user_id")]
        public int friend_user_id { get; set; }

        [Key]
        [Column("friend_befriend_user_id")]
        public int friend_befriend_user_id { get; set; }
    }
}
