using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BlackjackCommon.ViewModels
{
	public class FriendRequestModel
	{
		public int user_id { get; set; }

		public string user_name { get; set; }
		public bool can_answer { get; set; }
	}
}
