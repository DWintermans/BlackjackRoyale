using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlackjackCommon.Entities.History
{
	[Table("history")]
	public class History
	{
		[Key]
		[Column("history_id")]
		public int history_id { get; set; }

		[Column("history_group_id")]
		[StringLength(50)] 
		public string history_group_id { get; set; }

		[Column("history_user_id")]
		public int? history_user_id { get; set; } 

		[Column("history_action")]
		[Required]
		public HistoryAction history_action { get; set; }

		[Column("history_result")]
		public HistoryResult? history_result { get; set; } 

		[Column("history_payload")]
		public string history_payload { get; set; } 

		[Column("history_round_number")]
		public int history_round_number { get; set; }

		[Column("history_datetime")]
		public DateTime history_datetime { get; set; }
	}

	public enum HistoryAction
	{
		TURN,
		CREDITS_UPDATE,
		CARD_DRAWN,
		BET_PLACED,
		GAME_FINISHED,
		GAME_STARTED,
		PLAYER_FINISHED,
		INSURANCE_PAID,
		HIT,
		STAND,
		SPLIT,
		INSURE,
		DOUBLE,
		SURRENDER
	}

	public enum HistoryResult
	{
		BUSTED,
		WIN,
		LOSE,
		PUSH,
		BLACKJACK,
		SURRENDER
	} 
}
