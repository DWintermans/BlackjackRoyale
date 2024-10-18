using BlackjackCommon.Data.SharedData;
using BlackjackCommon.Interfaces.Logic;
using BlackjackCommon.Models;
using BlackjackCommon.ViewModels;
using System.Numerics;
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
		public event Func<Player, GameModel, Task>? OnGameInfoToPlayer;

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
					if (await IsCurrentPlayersTurn(player, group))
					{ 
						await Hit(player);
						await TryToFinishGame(group);
					}
					break;

				case "stand":
					if (await IsCurrentPlayersTurn(player, group))
					{
						await Stand(player);
						await TryToFinishGame(group);
					}
					break;

				default:
					await OnNotification?.Invoke(player, "Unknown game action", NotificationType.TOAST, ToastType.ERROR);
					break;
			}
		}

		private async Task TryToFinishGame(Group group) 
		{
			foreach (var member in group.Members)
			{
				if (!member.HasFinished)
				{
					return;
				}
			}

			while (int.Parse(CalculateHandValue(group.DealerHand)) <= 16) 
			{
				await DealCardToDealer(group);
			}

			foreach (var member in group.Members)
			{
				int memberHand = int.Parse(CalculateHandValue(member.Hand));
				int dealerHand = int.Parse(CalculateHandValue(group.DealerHand));

				//push / tie
				if (memberHand == dealerHand)
				{
					group.Bets.TryGetValue(member, out int bet);
					member.Credits += bet;

					GameModel model = new GameModel
					{
						User_ID = member.User_ID,
						Action = GameAction.GAME_FINISHED,
						Result = GameResult.PUSH
					};

					await OnGameInfoToGroup?.Invoke(group, model);
					continue;
				}

				//bust
				if (memberHand > 21) 
				{
					GameModel model = new GameModel
					{
						User_ID = member.User_ID,
						Action = GameAction.GAME_FINISHED,
						Result = GameResult.BUSTED
					};

					await OnGameInfoToGroup?.Invoke(group, model);
					continue;
				}

				//blackjack pays 3 to 2 (a.k.a. * 1.5)
				if (memberHand == 21)
				{
					group.Bets.TryGetValue(member, out int bet);

					int bonus = (int)(bet * 1.5);
					member.Credits += bonus;

					GameModel model = new GameModel
					{
						User_ID = member.User_ID,
						Action = GameAction.GAME_FINISHED,
						Result = GameResult.PUSH
					};

					await OnGameInfoToGroup?.Invoke(group, model);
					continue;
				}

				//lose
				if (dealerHand > memberHand && dealerHand <= 21) 
				{
					GameModel model = new GameModel
					{
						User_ID = member.User_ID,
						Action = GameAction.GAME_FINISHED,
						Result = GameResult.LOSE
					};

					await OnGameInfoToGroup?.Invoke(group, model);
					continue;
				}

				//win
				if (memberHand > dealerHand || dealerHand > 21)
				{
					group.Bets.TryGetValue(member, out int bet);

					int bonus = (int)(bet * 2);
					member.Credits += bonus;

					GameModel model = new GameModel
					{
						User_ID = member.User_ID,
						Action = GameAction.GAME_FINISHED,
						Result = GameResult.WIN
					};

					await OnGameInfoToGroup?.Invoke(group, model);
					continue;
				}
			}

			//send credits update privately
			foreach (var member in group.Members) 
			{
				GameModel model = new GameModel
				{
					User_ID = member.User_ID,
					Action = GameAction.CREDITS_UPDATE,
					Credits = member.Credits
				};

				await OnGameInfoToPlayer?.Invoke(member, model);
			}

			group.Status = Group.GroupStatus.BETTING;
			StartBetting(group);
		}

		private async Task<bool> IsCurrentPlayersTurn(Player player, Group group)
		{
			if (group.Status != Group.GroupStatus.PLAYING) return false;

			if (player.HasFinished) 
			{
				await OnNotification?.Invoke(player, "You have already finished your turn.", NotificationType.TOAST, ToastType.INFO);
				return false;
			}

			foreach (var member in group.Members)
			{
				if (member == player)
				{
					return true;
				} 
				else
				{	
					if (!member.HasFinished)
					{
						await OnNotification?.Invoke(player, "You must wait for other players to finish their turn.", NotificationType.TOAST, ToastType.WARNING);
						return false;
					}
				}
			}

			Console.WriteLine($"{player.Name} is not part of {group.Group_ID}");
			return false;
		}

		public async Task StartBetting(Group group) 
		{
			await _groupLogic.Value.MovePlayersFromWaitingRoom(group);

			await OnGroupNotification?.Invoke(group, "Place your bets now!", NotificationType.GAME, default);

			group.Bets.Clear();
			foreach (var member in group.Members)
			{
				member.HasFinished = false;
			}
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
				Total_Card_Value = CalculateHandValue(group.DealerHand)
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
			
			string totalHandValue = CalculateHandValue(player.Hand);

			GameModel model = new GameModel
			{
				User_ID = player.User_ID,
				Action = player.Hand.Count > 2 ? GameAction.HIT : GameAction.CARD_DRAWN,
				Card = cardName,
				Total_Card_Value = totalHandValue
			};

			await OnGameInfoToGroup?.Invoke(group, model);

			//end turn for player if above or equal to 21
			if (int.Parse(totalHandValue) > 21 || int.Parse(totalHandValue) == 21) 
			{
				player.HasFinished = true;
				GameModel finishedModel = new GameModel
				{
					User_ID = player.User_ID,
					Action = GameAction.PLAYER_FINISHED,
				};

				//notify about game-action (player finished playing)
				await OnGameInfoToGroup?.Invoke(group, finishedModel);
			}

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
				Total_Card_Value = CalculateHandValue(group.DealerHand)
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
				User_ID = player.User_ID,
				Action = GameAction.STAND,
				Total_Card_Value = CalculateHandValue(player.Hand)
			};

			//notify about game-action (stand)
			await OnGameInfoToGroup?.Invoke(group, model);

			player.HasFinished = true;
			GameModel model2 = new GameModel
			{
				User_ID = player.User_ID,
				Action = GameAction.PLAYER_FINISHED,
			};
			
			//notify about game-action (player finished playing)
			await OnGameInfoToGroup?.Invoke(group, model2);
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

			//all bets locked in? start game
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