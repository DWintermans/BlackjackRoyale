namespace BlackjackCommon.ViewModels
{
	public class GameModel
	{
		public GameAction Action { get; set; }
		public int User_ID { get; set; } //0 for dealer, otherwise user_id
		public string? Card { get; set; }
		public string? Total_Card_Value { get; set; }
		public int? Bet { get; set; }
		public int? Credits { get; set; }
		public GameResult? Result { get; set; }
	}

	public enum GameAction
	{
		CREDITS_UPDATE,
		CARD_DRAWN,
		BET_PLACED,
		GAME_FINISHED,
		PLAYER_FINISHED,
		HIT,
		STAND,
		SPLIT,
		INSURE,		
	}

	public enum GameResult
	{
		BUSTED, //21+
		WIN,
		LOSE,
		PUSH, //tie
	}
}

