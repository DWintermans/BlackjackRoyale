﻿namespace BlackjackService
{
	public class LobbyModel
	{
		public string Type { get; set; }
		public List<Lobby> Lobby { get; set; } = new List<Lobby>();
	}

	public class Lobby
	{
		public string Group_ID { get; set; }
		public int Members { get; set; }
	}
}