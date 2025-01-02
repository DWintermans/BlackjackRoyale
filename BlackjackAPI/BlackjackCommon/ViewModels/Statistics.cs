using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BlackjackCommon.ViewModels
{
	public class LeaderboardModel
	{
		public required string user_name { get; set; }
		public string ratio { get; set; }
	}
}
