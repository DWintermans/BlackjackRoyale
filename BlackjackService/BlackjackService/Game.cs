namespace BlackjackService
{
	public class Game
	{
		private static Dictionary<string, List<string>> dealerHand = new Dictionary<string, List<string>>(); //group_id, cards

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

		public static async Task HandleGameAction(dynamic message, int user_id)
		{
			switch (message.action.ToString())
			{
				case "bet":
					//await Bet(user_id, message.bet.ToString());

				case "hit":
					await Hit(user_id);
					break;

				case "stand":
					await Stand(user_id);
					break;

				default:
					await Websocket.SendNotificationToUserID(user_id, "Unknown game action");
					break;
			}
		}
		public static async Task StartGame(string group_id)
		{
			await Group.MovePlayersFromWaitingRoom(group_id);

			await Websocket.SendNotificationToGroupID(group_id, "Place your bets now!");

			while (!SharedData.groupDeck.ContainsKey(group_id) || SharedData.groupDeck[group_id].Count <= 52)
			{
				AddNewDeckToGroup(group_id);
			}

			if (SharedData.groupDeck[group_id].Count <= 52)
			{
				AddNewDeckToGroup(group_id);
			}

			if (SharedData.groupMembers.TryGetValue(group_id, out List<int> groupMembers))
			{
				//initialize game, clear hands
				foreach (int user_id in groupMembers)
				{
					if (!SharedData.playerHands.ContainsKey(user_id))
					{
						SharedData.playerHands[user_id] = new List<string>();
					}
					else
					{
						//clear player hand if possible
						SharedData.playerHands[user_id].Clear();
					}
				}

				if (!dealerHand.ContainsKey(group_id))
				{
					dealerHand[group_id] = new List<string>();
				}
				else
				{
					//clear dealer hand if possible
					dealerHand[group_id].Clear();
				}

				//give each player a card
				foreach (int user_id in groupMembers)
				{
					await DealCard(user_id);
				}

				//give dealer a card
				await DealCardToDealer(group_id);

				//give each player a second card
				foreach (int user_id in groupMembers)
				{
					await DealCard(user_id);
				}

				await Websocket.SendNotificationToGroupID(group_id, "Setup has ended");
				
//TODO: send message for displaying fake second card dealer after starting game
			}
		}

		private static void AddNewDeckToGroup(string group_id)
		{
			Random rng = new Random();
			List<string> newDeck = new List<string>(baseDeck);
			newDeck = newDeck.OrderBy(card => rng.Next()).ToList();

			if (!SharedData.groupDeck.ContainsKey(group_id))
			{
				SharedData.groupDeck[group_id] = new List<string>();
			}

			//add deck to existing
			SharedData.groupDeck[group_id].AddRange(newDeck); 

			Console.WriteLine($"A new deck has been shuffled and added to group: {group_id}");
		}

		private static async Task DealCard(int user_id)
		{
			string group_id = GetGroupIDForUserID(user_id);

			if (SharedData.groupDeck[group_id].Count == 0) return;

			string card = SharedData.groupDeck[group_id][0];
			SharedData.groupDeck[group_id].RemoveAt(0);

			//remove first character (e.g. H9 > 9, HK > 10, H0 > 10)
			char cardRank = card[1];
			cardToValueMap.TryGetValue(cardRank, out int cardvalue);
			cardToNameMap.TryGetValue(card, out string cardName);

			SharedData.playerHands[user_id].Add(cardvalue.ToString());

			await Websocket.SendNotificationToUserID(user_id, $"You were dealt: {cardName}");
			Console.WriteLine($"{user_id} received {cardName}");
		}

		private static async Task DealCardToDealer(string group_id)
		{
			if (SharedData.groupDeck[group_id].Count == 0) return;

			string card = SharedData.groupDeck[group_id][0];
			SharedData.groupDeck[group_id].RemoveAt(0);

			//remove first character (e.g. H9 > 9, HK > 10, H0 > 10)
			char cardRank = card[1];
			cardToValueMap.TryGetValue(cardRank, out int cardvalue);
			cardToNameMap.TryGetValue(card, out string cardName);

			dealerHand[group_id].Add(card);

			await Websocket.SendNotificationToGroupID(group_id, $"Dealer was dealt: {cardName}");
			Console.WriteLine($"{group_id} dealer received {cardName}");
		}

		private static async Task Hit(int user_id)
		{
			await DealCard(user_id);		
		}
		
		private static async Task Stand(int user_id)
		{
			await StartGame(GetGroupIDForUserID(user_id));
		}

		private static string GetGroupIDForUserID(int user_id)
		{
			foreach (var group in SharedData.groupMembers)
			{
				if (group.Value.Contains(user_id))
				{
					return group.Key;
				}
			}
			return null;
		}

	}
}