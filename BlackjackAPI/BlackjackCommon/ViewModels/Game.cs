namespace BlackjackCommon.ViewModels
{
	public class GameModel
	{
		public GameAction Action { get; set; }
		public int User_ID { get; set; } //0 for dealer, otherwise user_id
		public int? Hand {  get; set; } //min 1 to max 4 (if split)
		public string? Card { get; set; }
		public string? Total_Card_Value { get; set; }
		public int? Total_Bet_Value { get; set; }
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
		PLAYER_JOINED,
		PLAYER_LEFT,
		INSURANCE_PAID,
		HIT,
		STAND,
		SPLIT,
		INSURE,	
		DOUBLE,
		SURRENDER,
	}

	public enum GameResult
	{
		BUSTED, //21+
		WIN,
		LOSE,
		PUSH, //tie
		BLACKJACK, //21 with 2 cards
		SURRENDER,
	}
}

