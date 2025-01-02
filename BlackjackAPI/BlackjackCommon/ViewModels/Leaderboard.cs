using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BlackjackCommon.ViewModels
{
	public class StatisticsModel
	{
		public required string balance { get; set; }
		public int? games_played { get; set; }
		public int? total_game_wins { get; set; }
		public int? total_game_losses { get; set; }
		public int? total_earnings { get; set; }
		public int? total_losses { get; set; }
		public string? playtime { get; set; }

		public int? blackjack_achieved { get; set; }
		public int? tied { get; set; }
		public int? surrendered { get; set; }
		public int? split { get; set; }
		public int? doubled { get; set; }
		public int? used_insurance { get; set; }
		public int? received_insurance { get; set; }

		public int? longest_winning_streak { get; set; }
		public int? longest_losing_streak { get; set; }
		public int? highest_game_win { get; set; }
		public int? highest_game_loss { get; set; }
	}
}
