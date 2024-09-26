namespace BlackjackService
{
	public class Game
	{
		static List<string> baseDeck = new List<string>
		{
			"H2", "H3", "H4", "H5", "H6", "H7", "H8", "H9", "H0", "HJ", "HK", "HQ", "HA",
			"S2", "S3", "S4", "S5", "S6", "S7", "S8", "S9", "S0", "SJ", "SK", "SQ", "SA",
			"D2", "D3", "D4", "D5", "D6", "D7", "D8", "D9", "D0", "DJ", "DK", "DQ", "DA",
			"C2", "C3", "C4", "C5", "C6", "C7", "C8", "C9", "C0", "CJ", "CK", "CQ", "CA"
		};

		//card values to numeric values
		static Dictionary<char, int> cardToValueMap = new Dictionary<char, int>
		{
			{'2', 2}, {'3', 3}, {'4', 4}, {'5', 5}, {'6', 6}, {'7', 7}, {'8', 8}, {'9', 9}, {'0', 10},
			{'K', 10}, {'Q', 10}, {'J', 10}, {'A', 11}
		};

		//map card values to file img names
		static Dictionary<string, string> cardToNameMap = new Dictionary<string, string>
		{
			{"H2", "TwoHearts.png"}, {"H3", "ThreeHearts.png"}, {"H4", "FourHearts.png"}, {"H5", "FiveHearts.png"},
			{"H6", "SixHearts.png"}, {"H7", "SevenHearts.png"}, {"H8", "EightHearts.png"}, {"H9", "NineHearts.png"}, {"H0", "TenHearts.png"},
			{"S2", "TwoSpades.png"}, {"S3", "ThreeSpades.png"}, {"S4", "FourSpades.png"}, {"S5", "FiveSpades.png"},
			{"S6", "SixSpades.png"}, {"S7", "SevenSpades.png"}, {"S8", "EightSpades.png"}, {"S9", "NineSpades.png"}, {"S0", "TenSpades.png"},
			{"D2", "TwoDiamonds.png"}, {"D3", "ThreeDiamonds.png"}, {"D4", "FourDiamonds.png"}, {"D5", "FiveDiamonds.png"},
			{"D6", "SixDiamonds.png"}, {"D7", "SevenDiamonds.png"}, {"D8", "EightDiamonds.png"}, {"D9", "NineDiamonds.png"}, {"D0", "TenDiamonds.png"},
			{"C2", "TwoClubs.png"}, {"C3", "ThreeClubs.png"}, {"C4", "FourClubs.png"}, {"C5", "FiveClubs.png"},
			{"C6", "SixClubs.png"}, {"C7", "SevenClubs.png"}, {"C8", "EightClubs.png"}, {"C9", "NineClubs.png"}, {"C0", "TenClubs.png"},
			{"SK", "KingSpades.png"}, {"SQ", "QueenSpades.png"}, {"SA", "AceSpades.png"},
			{"HK", "KingHearts.png"}, {"HQ", "QueenHearts.png"}, {"HA", "AceHearts.png"},
			{"DK", "KingDiamonds.png"}, {"DQ", "QueenDiamonds.png"}, {"DA", "AceDiamonds.png"},
			{"CK", "KingClubs.png"}, {"CQ", "QueenClubs.png"}, {"CA", "AceClubs.png"},
			{"HJ", "JackHearts.png"}, {"CJ", "JackClubs.png"}, {"DJ", "JackDiamonds.png"}, {"SJ", "JackSpades.png"}
		};

		public static async Task HandleGameAction(Player player, dynamic message)
		{
			//check if group exists / if game has started (has received deck)
			Group group = SharedData.GetGroupForPlayer(player);

			if (group == null)
			{
				await Websocket.SendNotificationToPlayer(player, "You must be part of a group to play the game.");
				return;
			}

			if (group.Deck.Count == 0) 
			{
				await Websocket.SendNotificationToPlayer(player, "The game has not started yet");
				return;
			}

			switch (message.action.ToString())
			{
				case "bet":
					//await Bet(player, message.bet.ToString());

				case "hit":
					await Hit(player);
					break;

				case "stand":
					await Stand(player);
					break;

				default:
					await Websocket.SendNotificationToPlayer(player, "Unknown game action");
					break;
			}
		}
		public static async Task StartGame(Group group)
		{
			await Group.MovePlayersFromWaitingRoom(group);

			await Websocket.SendNotificationToGroup(group, "Place your bets now!");

			while (group.Deck.Count <= 52)
			{
				AddNewDeckToGroup(group);
			}

			//clear player hand
			foreach (var player in group.Members)
			{
				player.ClearHand();
			}

			//clear dealer hand
			group.DealerHand.Clear();

			//give each player a card
			foreach (var player in group.Members)
			{
				await DealCard(player);
			}

			//give dealer a card
			await DealCardToDealer(group);

			//give each player a second card
			foreach (var player in group.Members)
			{
				await DealCard(player);
			}

			await Websocket.SendNotificationToGroup(group, "Setup has ended");

			//TODO: send message for displaying fake second card dealer after starting game
		}

		private static void AddNewDeckToGroup(Group group)
		{
			Random rng = new Random();
			List<string> newDeck = new List<string>(baseDeck);
			newDeck = newDeck.OrderBy(card => rng.Next()).ToList();

			//add deck to existing
			group.Deck.AddRange(newDeck);

			Console.WriteLine($"A new deck has been shuffled and added to group: {group.Group_ID}");
		}

		private static async Task DealCard(Player player)
		{
			Group group = SharedData.GetGroupForPlayer(player);

			if (group.Deck.Count == 0) return;

			string card = group.Deck[0];
			group.Deck.RemoveAt(0);

			//remove first character (e.g. H9 > 9, HK > 10, H0 > 10)
			char cardRank = card[1];
			cardToValueMap.TryGetValue(cardRank, out int cardvalue);
			cardToNameMap.TryGetValue(card, out string cardName);

			player.Hand.Add(cardvalue.ToString());

			await Websocket.SendNotificationToPlayer(player, $"You were dealt: {cardName}");
			Console.WriteLine($"{player.User_ID} received {cardName}");
		}

		private static async Task DealCardToDealer(Group group)
		{
			if (group.Deck.Count == 0) return;

			string card = group.Deck[0];
			group.Deck.RemoveAt(0);

			//remove first character (e.g. H9 > 9, HK > 10, H0 > 10)
			char cardRank = card[1];
			cardToValueMap.TryGetValue(cardRank, out int cardvalue);
			cardToNameMap.TryGetValue(card, out string cardName);

			group.DealerHand.Add(card);

			await Websocket.SendNotificationToGroup(group, $"Dealer was dealt: {cardName}");
			Console.WriteLine($"{group.Group_ID} dealer received {cardName}");
		}

		private static async Task Hit(Player player)
		{
			await DealCard(player);		
		}
		
		private static async Task Stand(Player player)
		{
			await StartGame(SharedData.GetGroupForPlayer(player));
		}

	}
}