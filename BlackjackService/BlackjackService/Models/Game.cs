﻿namespace BlackjackService
{
	public class GameModel
	{
		public GameAction Action { get; set; }
		public int User_ID { get; set; } //0 for dealer, otherwise user_id
		public string Card { get; set; }
		public int? Total { get; set; }
		public GameResult? Result { get; set; }
	}
	public enum GameAction
	{
		CARD_DRAWN,
		BET_PLACED,
		GAME_OVER,
	}

	public enum GameResult
	{
		BUSTED, //21+
		WIN,
		LOSE,
		PUSH, //tie
	}
}
