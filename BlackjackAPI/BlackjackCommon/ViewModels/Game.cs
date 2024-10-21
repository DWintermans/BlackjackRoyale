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
		public int? Cards_In_Deck { get; set; }
		public GameResult? Result { get; set; }
	}

	public enum GameAction
	{
		TURN, //shows whose turn it is
		CREDITS_UPDATE,
		CARD_DRAWN, //the default 2 cards given to every player
		BET_PLACED,
		GAME_FINISHED,
		GAME_STARTED,
		PLAYER_FINISHED,
		HIT,
		STAND,
		SPLIT,
		INSURE,	
		DOUBLE,
	}

	public enum GameResult
	{
		BUSTED, //21+
		WIN,
		LOSE,
		PUSH, //tie
		BLACKJACK, //21 with 2 cards
	}
}

