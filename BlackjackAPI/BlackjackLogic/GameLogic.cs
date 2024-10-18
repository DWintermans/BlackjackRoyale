using BlackjackCommon.Data.SharedData;
using BlackjackCommon.Interfaces.Logic;
using BlackjackCommon.Models;
using BlackjackCommon.ViewModels;
using System.Threading.Tasks.Dataflow;
using Group = BlackjackCommon.Models.Group;
using Player = BlackjackCommon.Models.Player;

namespace BlackjackLogic
{
	public class GameLogic : IGameLogic
	{
		public event Func<Player, string, NotificationType, ToastType?, Task>? OnNotification;
		public event Func<Group, string, NotificationType, ToastType?, Task> OnGroupNotification;
		public event Func<Group, GameModel, Task>? OnGameInfoToGroup;

		private readonly Lazy<IGroupLogic> _groupLogic;
		private readonly Lazy<IPlayerLogic> _playerLogic;

		public GameLogic(Lazy<IGroupLogic> groupLogic, Lazy<IPlayerLogic> playerLogic)
		{
			_groupLogic = groupLogic;
			_playerLogic = playerLogic;
		}

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

		public async Task HandleGameAction(Player player, dynamic message)
		{
			//check if group exists / if game has started (has received deck)
			Group group = SharedData.GetGroupForPlayer(player);

			if (group == null)
			{
				await OnNotification?.Invoke(player, "You must be part of a group to play the game.", NotificationType.TOAST, ToastType.INFO);
				return;
			}

			if (group.Status == Group.GroupStatus.WAITING)
			{
				await OnNotification?.Invoke(player, "The game has not started yet", NotificationType.TOAST, ToastType.INFO);
				return;
			}

			switch (message.action.ToString())
			{
				case "bet":
					await Bet(player, message.bet.ToString());
					break;

				case "hit":
					await CheckPlayingOrder(player);
					await Hit(player);
					break;

				case "stand":
					await CheckPlayingOrder(player);
					await Stand(player);
					break;

				default:
					await OnNotification?.Invoke(player, "Unknown game action", NotificationType.TOAST, ToastType.ERROR);
					break;
			}
		}

		public async Task CheckPlayingOrder(Player player) 
		{
			
		}

		public async Task StartBetting(Group group) 
		{
			await _groupLogic.Value.MovePlayersFromWaitingRoom(group);

			await OnGroupNotification?.Invoke(group, "Place your bets now!", NotificationType.GAME, default);
		}

		public async Task StartGame(Group group)
		{
			await OnGroupNotification?.Invoke(group, "Game is starting now!", NotificationType.GAME, default);

			//shuffle and play with two decks, when starting round and one deck is depleted start game with 2 new shuffled decks 
			while (group.Deck.Count <= 52)
			{
				RemoveOldDecksAndAddTwoDecksToGroup(group);
			}

			//clear player hand
			foreach (var player in group.Members)
			{
				_playerLogic.Value.ClearHand(player);
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

			//give faced down card to dealer
			GameModel model = new GameModel
			{
				User_ID = 0,
				Action = GameAction.CARD_DRAWN,
				Card = "CardDown.png",
				Total = CalculateHandValue(group.DealerHand)
			};

			await OnGameInfoToGroup?.Invoke(group, model);

			await OnGroupNotification?.Invoke(group, "Setup has ended", NotificationType.GAME, default);
		}

		private void RemoveOldDecksAndAddTwoDecksToGroup(Group group)
		{
			group.Deck.Clear();

			Random rng = new Random();

			//add 2 shuffled decks
			for (int i = 0; i < 2; i++)
			{
				List<string> newDeck = new List<string>(baseDeck);
				newDeck = newDeck.OrderBy(card => rng.Next()).ToList();
				group.Deck.AddRange(newDeck);
			}

			Console.WriteLine($"Two new decks have been shuffled and added to group: {group.Group_ID}");
		}

		private async Task DealCard(Player player)
		{
			Group group = SharedData.GetGroupForPlayer(player);

			if (group.Status != Group.GroupStatus.PLAYING) return;

			string card = group.Deck[0];
			group.Deck.RemoveAt(0);

			//remove first character (e.g. H9 > 9, HK > 10, H0 > 10)
			char cardRank = card[1];
			cardToValueMap.TryGetValue(cardRank, out int cardvalue);
			cardToNameMap.TryGetValue(card, out string cardName);

			player.Hand.Add(cardvalue.ToString());

			GameModel model = new GameModel
			{
				User_ID = player.User_ID,
				Action = GameAction.CARD_DRAWN,
				Card = cardName,
				Total = CalculateHandValue(player.Hand)
			};

			await OnGameInfoToGroup?.Invoke(group, model);

			Console.WriteLine($"{player.User_ID} received {cardName}, value in hand: {CalculateHandValue(player.Hand)}");
		}

		private async Task DealCardToDealer(Group group)
		{
			if (group.Status != Group.GroupStatus.PLAYING) return;

			string card = group.Deck[0];
			group.Deck.RemoveAt(0);

			//remove first character (e.g. H9 > 9, HK > 10, H0 > 10)
			char cardRank = card[1];
			cardToValueMap.TryGetValue(cardRank, out int cardvalue);
			cardToNameMap.TryGetValue(card, out string cardName);

			group.DealerHand.Add(cardvalue.ToString());

			GameModel model = new GameModel
			{
				User_ID = 0,
				Action = GameAction.CARD_DRAWN,
				Card = cardName,
				Total = CalculateHandValue(group.DealerHand)
			};

			await OnGameInfoToGroup?.Invoke(group, model);
			Console.WriteLine($"{group.Group_ID} dealer received {cardName}, value in hand: {CalculateHandValue(group.DealerHand)}");
		}

		private async Task Hit(Player player)
		{
			await DealCard(player);
		}

		private async Task Stand(Player player)
		{
			Group group = SharedData.GetGroupForPlayer(player);

			GameModel model = new GameModel
			{
				User_ID = 0,
				Action = GameAction.STAND,
				Total = CalculateHandValue(player.Hand)
			};

			await OnGameInfoToGroup?.Invoke(group, model);
		}

		private async Task Bet(Player player, string bet_amount) 
		{
			if (!int.TryParse(bet_amount, out int bet)) {
				await OnNotification?.Invoke(player, "Unexpected bet value received", NotificationType.TOAST, ToastType.ERROR);
				return;
			}

			Group group = SharedData.GetGroupForPlayer(player);

			if (group.Status != Group.GroupStatus.BETTING)
			{
				await OnNotification?.Invoke(player, "Betting is not allowed at this stage.", NotificationType.TOAST, ToastType.WARNING);
				return;
			}

			if (group.Bets.ContainsKey(player))
			{
				await OnNotification?.Invoke(player, "You have already placed your bet.", NotificationType.TOAST, ToastType.WARNING);
				return;
			}

			if (player.Credits < bet) 
			{
				await OnNotification?.Invoke(player, $"You can't bet more than you have. You have {player.Credits} credits remaining.", NotificationType.TOAST, ToastType.WARNING);
				return;
			}

			group.Bets.Add(player, bet);

			player.Credits -= bet;

			GameModel model = new GameModel
			{
				User_ID = player.User_ID,
				Action = GameAction.BET_PLACED,
				Bet = bet,
			};

			await OnGameInfoToGroup?.Invoke(group, model);

			//all bets locked? start game
			if (group.Bets.Count == group.Members.Count)
			{
				group.Status = Group.GroupStatus.PLAYING;
				StartGame(group);
			}
		}

		private string CalculateHandValue(List<string> hand)
		{
			int totalValue = 0;
			int acesCount = 0;

			foreach (string card in hand)
			{
				switch (card)
				{
					case "11":
						acesCount++;
						break;
					default:
						totalValue += int.Parse(card);
						break;
				}
			}

			if (acesCount > 0)
			{
				int aceAsEleven = totalValue + acesCount + 10;

				if (aceAsEleven <= 21)
				{
					return $"{totalValue + acesCount}/{aceAsEleven}";
				}
			}

			return (totalValue + acesCount).ToString();
		}
	}
}