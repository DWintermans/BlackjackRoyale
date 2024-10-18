namespace BlackjackCommon.ViewModels
{
	public class GameModel
	{
		public GameAction Action { get; set; }
		public int User_ID { get; set; } //0 for dealer, otherwise user_id
		public string? Card { get; set; }
		public string? Total { get; set; }
		public int? Bet { get; set; }
		public GameResult? Result { get; set; }
	}

	public enum GameAction
	{
		CARD_DRAWN,
		BET_PLACED,
		GAME_OVER,
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

