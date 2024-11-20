using BlackjackCommon.Data.SharedData;
using BlackjackCommon.Interfaces.Logic;
using BlackjackCommon.Models;
using BlackjackCommon.ViewModels;
using System;
using System.Collections.Immutable;
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

		//messages for bankruptcy 
		string[] bankruptMessages = new string[]
		{
			"Well, looks like you went all-in... and lost it all! Lucky for you, we’re feeling generous. Here’s a second chance – don’t blow it!",
			"Ouch, that last hand cleaned you out! Good thing this isn’t Vegas, and we're handing out free credits. Go ahead, give it another shot!",
			"Congratulations! You hit rock bottom! Here’s some pity credits to get you back in the game. Remember, they don’t grow on trees… or do they?",
			"They say you’ve gotta lose it all to start fresh. Here’s a little something to get you back in the game – now go win it all back!",
			"Welcome to the Bank of Pity, where bankrupt players get a second chance! We’re loading you up with a few credits. Don’t spend it all in one place (or do)!",
			"You’re officially broke! But hey, we all deserve a second chance. Here’s your freebie – maybe this time, play it a bit cooler?",
			"Looks like the house won! Again. Here’s a little boost to keep you in the game… but remember, the house always… well, you know the rest!",
			"You did it! You hit zero! Here’s some free credits to save face. Maybe this time, try a little less… enthusiasm.",
			"Even the best players go broke sometimes. Here’s a fresh stack of credits to turn it around. Now go make that comeback!"
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
				await OnNotification?.Invoke(player, "The game has not started yet.", NotificationType.TOAST, ToastType.INFO);
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

				case "double":
					if (await IsCurrentPlayersTurn(player, group))
					{
						await Double(player);
						await TryToFinishGame(group);
					}
					break;

				case "surrender":
					if (await IsCurrentPlayersTurn(player, group))
					{
						await Surrender(player);
						await TryToFinishGame(group);
					}
					break;

				case "insure":
					if (await IsCurrentPlayersTurn(player, group))
					{
						await Insure(player);
						await TryToFinishGame(group);
					}
					break;

				case "split":
					if (await IsCurrentPlayersTurn(player, group))
					{
						await Split(player);
						await TryToFinishGame(group);
					}
					break;

				default:
					await OnNotification?.Invoke(player, "Unknown game action", NotificationType.TOAST, ToastType.ERROR);
					break;
			}
		}

		private async Task WhoseTurnIsIt(Group group) 
		{
			foreach (var member in group.Members)
			{
				if (!member.HasFinished)
				{
					var (activeHand, index) = GetActiveHand(member);

					if (activeHand == null)
					{
						await OnGroupNotification?.Invoke(group, "An error occured, please try again later.", NotificationType.GAME, default);
						Console.WriteLine($"{member.User_ID} has no active hands to deal a card.");
						return;
					}

					GameModel model = new GameModel
					{
						User_ID = member.User_ID,
						Action = GameAction.TURN,
						Hand = index + 1
					};

					await OnGameInfoToGroup?.Invoke(group, model);

					return;
				}
			}
		}

		private async Task TryToFinishGame(Group group) 
		{
			foreach (var member in group.Members)
			{
				if (!member.HasFinished)
				{
					WhoseTurnIsIt(group);
					return;
				}
			}

			//dealers' turn
			GameModel turnModel = new GameModel
			{
				User_ID = 0,
				Action = GameAction.TURN,
				Hand = 1
			};

			await OnGameInfoToGroup?.Invoke(group, turnModel);

			await Task.Delay(1000);

			//display faced down card
			GameModel gameModel = new GameModel
			{
				User_ID = 0,
				Action = GameAction.CARD_DRAWN,
				Card = group.HoleCard,
				Total_Card_Value = CalculateHandValue(group.DealerHand),
				Cards_In_Deck = group.Deck.Count,
				Hand = 1
			};

			await OnGameInfoToGroup?.Invoke(group, gameModel);

			while (GetBestHandValue((CalculateHandValue(group.DealerHand))) <= 16) 
			{				
				await Task.Delay(1000);
				await DealCardToDealer(group);
			}

			foreach (var member in group.Members)
			{
				int gameWins = 0;
				int gameLosses = 0;
				int earnings = 0;
				int losses = 0;

				for (int i = 0; i < member.Hands.Count; i++)
				{
					var hand = member.Hands[i];

					int memberHand = GetBestHandValue(CalculateHandValue(hand.Cards));
					int dealerHand = GetBestHandValue((CalculateHandValue(group.DealerHand)));

					group.Bets.TryGetValue(member, out int bet);

					if (hand.IsDoubled) 
					{
						bet = bet * 2;
					}

					//surrendered? no cards so value is 0
					if (memberHand == 0)
					{
						gameLosses += 1;
						losses += bet / 2;

						GameModel model = new GameModel
						{
							User_ID = member.User_ID,
							Action = GameAction.GAME_FINISHED,
							Result = GameResult.SURRENDER,
							Hand = i + 1,
							Bet = bet / 2 //show winnings/losses
						};

						await OnGameInfoToGroup?.Invoke(group, model);
						continue;
					}

					//dealer has blackjack, payout insurance
					if (dealerHand == 21 && group.DealerHand.Count == 2)
					{
						if (member.HasInsurance)
						{
							earnings += bet / 2;

							member.Credits += bet;
							GameModel model = new GameModel
							{
								User_ID = member.User_ID,
								Action = GameAction.INSURANCE_PAID,
								Bet = bet
							};

							await OnGameInfoToGroup?.Invoke(group, model);
						}
					}
					else 
					{
						if (member.HasInsurance) 
						{
							losses += bet / 2;
						}
					}

					//bust
					if (memberHand > 21)
					{
						gameLosses += 1;
						losses += bet;

						GameModel model = new GameModel
						{
							User_ID = member.User_ID,
							Action = GameAction.GAME_FINISHED,
							Result = GameResult.BUSTED,
							Hand = i + 1,
							Bet = bet
						};

						await OnGameInfoToGroup?.Invoke(group, model);
						continue;
					}

					//push / tie
					if (memberHand == dealerHand)
					{
						member.Credits += bet;

						GameModel model = new GameModel
						{
							User_ID = member.User_ID,
							Action = GameAction.GAME_FINISHED,
							Result = GameResult.PUSH,
							Bet = 0,
							Hand = i + 1,
						};

						await OnGameInfoToGroup?.Invoke(group, model);
						continue;
					}

					//blackjack pays 3 to 2 (a.k.a. * 1.5), only counts as blackjack if 21 is achieved with 2 cards
					if (memberHand == 21 && hand.Cards.Count == 2)
					{
						int bonus = (int)(bet * 0.5);

						gameWins += 1;
						earnings += bet + bonus;

						member.Credits += bet + bet + bonus;


						GameModel model = new GameModel
						{
							User_ID = member.User_ID,
							Action = GameAction.GAME_FINISHED,
							Result = GameResult.BLACKJACK,
							Bet = bet + bonus, //show winnings
							Hand = i + 1
						};

						await OnGameInfoToGroup?.Invoke(group, model);
						continue;
					}

					//lose
					if (dealerHand > memberHand && dealerHand <= 21)
					{
						gameLosses += 1;
						losses += bet;

						GameModel model = new GameModel
						{
							User_ID = member.User_ID,
							Action = GameAction.GAME_FINISHED,
							Result = GameResult.LOSE,
							Hand = i + 1,
							Bet = bet
						};

						await OnGameInfoToGroup?.Invoke(group, model);
						continue;
					}

					//win
					if (memberHand > dealerHand || dealerHand > 21)
					{
						gameWins += 1;
						earnings += bet;

						member.Credits += bet + bet;

						GameModel model = new GameModel
						{
							User_ID = member.User_ID,
							Action = GameAction.GAME_FINISHED,
							Result = GameResult.WIN,
							Bet = bet, //show winnings
							Hand = i + 1
						};

						await OnGameInfoToGroup?.Invoke(group, model);
						continue;
					}
				}

				_playerLogic.Value.UpdateStatistics(member, gameWins, gameLosses, earnings, losses);
				
			}

			//send credits update privately
			foreach (var member in group.Members) 
			{
				//prevent bankruptcy, send funny message
				if (member.Credits < 10) 
				{
					member.Credits += 100;
					Random random = new Random();
					string message = bankruptMessages[random.Next(bankruptMessages.Length)];
					string messageWithCredits = $"{message} [+100 credits]";
					await OnNotification?.Invoke(member, messageWithCredits, NotificationType.TOAST, ToastType.DEFAULT);
				}

				_playerLogic.Value.UpdateCredits(member, member.Credits);

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

		private int GetBestHandValue(string handValue)
		{
			if (handValue.Contains("/"))
			{
				var values = handValue.Split('/');
				int lowest = int.Parse(values[0]);
				int highest = int.Parse(values[1]);

				//return best value for player
				return highest <= 21 ? highest : lowest;
			}

			return int.Parse(handValue);
		}

		private async Task<bool> IsCurrentPlayersTurn(Player player, Group group)
		{
			if (group.Status != Group.GroupStatus.PLAYING) 
			{
				await OnNotification?.Invoke(player, "You must wait for the game to start to perform this action.", NotificationType.TOAST, ToastType.WARNING);
				return false;
			}

			//prevent spamming cards and ruining the delayed cards.
			if (group.DealerHand.Count < 2) 
			{
				await OnNotification?.Invoke(player, "You must wait for everyone to receive their cards.", NotificationType.TOAST, ToastType.WARNING);
				return false;
			}

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

			foreach (var member in group.Members)
			{
				GameModel startModel = new GameModel
				{
					User_ID = member.User_ID,
					Action = GameAction.GAME_STARTED,
				};

				await OnGameInfoToGroup?.Invoke(group, startModel);
			}

			//shuffle and play with two decks, when starting round and one deck is depleted start game with 2 new shuffled decks 
			while (group.Deck.Count <= 52)
			{
				RemoveOldDecksAndAddTwoDecksToGroup(group);
			}

			//clear player hand
			foreach (var player in group.Members)
			{
				player.HasInsurance = false;
				player.Hands.Clear();
				player.Hands.Add(new Player.Hand());
			}

			//clear dealer hand
			group.DealerHand.Clear();

			//give each player a card
			foreach (var player in group.Members.ToList()) 
			{
				if (!group.Members.Contains(player)) 
				{
					Console.WriteLine($"Player {player.User_ID} has left the group during the card distribution.");
					continue; 
				}

				await DealCard(player);
				await Task.Delay(1000);
			}

			//give dealer a card
			await DealCardToDealer(group);
			await Task.Delay(1000);

			//give each player a second card
			foreach (var player in group.Members.ToList())
			{
				if (!group.Members.Contains(player))
				{
					Console.WriteLine($"Player {player.User_ID} has left the group during the card distribution.");
					continue;
				}

				await DealCard(player);
				await Task.Delay(1000);
			}

			//display faced down card to dealer
			GameModel model = new GameModel
			{
				User_ID = 0,
				Action = GameAction.CARD_DRAWN,
				Card = "CardDown.png",
				Total_Card_Value = CalculateHandValue(group.DealerHand),
				Cards_In_Deck = Math.Max(1, group.Deck.Count - 1),
				Hand = 1
			};

			//give second card but dont display, players are allowed to hit/stand when 2 cards are in hand of dealer
			string card = group.Deck[0];
			group.Deck.RemoveAt(0);

			//remove first character (e.g. H9 > 9, HK > 10, H0 > 10)
			char cardRank = card[1];
			cardToValueMap.TryGetValue(cardRank, out int cardvalue);
			cardToNameMap.TryGetValue(card, out string cardName);

			group.DealerHand.Add(cardvalue.ToString());
			group.HoleCard = cardName;

			await OnGameInfoToGroup?.Invoke(group, model);

			await OnGroupNotification?.Invoke(group, "Setup has ended", NotificationType.GAME, default);

			//in case player gets blackjack, check if already done
			await TryToFinishGame(group);
		}

		private async void RemoveOldDecksAndAddTwoDecksToGroup(Group group)
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
			await OnGroupNotification?.Invoke(group, "Two new decks have been shuffled and added.", NotificationType.GAME, default);
		}

		private (Player.Hand hand, int index) GetActiveHand(Player player)
		{
			for (int i = 0; i < player.Hands.Count; i++)
			{
				if (!player.Hands[i].IsFinished)
				{
					return (player.Hands[i], i);
				}
			}

			return (null, -1);
		}

		private async Task DealCard(Player player)
		{
			Group group = SharedData.GetGroupForPlayer(player);

			if (group.Status != Group.GroupStatus.PLAYING) return;

			var (activeHand, index) = GetActiveHand(player);

			if (activeHand == null)
			{
				await OnGroupNotification?.Invoke(group, "An error occured, please try again later.", NotificationType.GAME, default);
				Console.WriteLine($"{player.User_ID} has no active hands to deal a card.");
				return;
			}

			string card = group.Deck[0];
			group.Deck.RemoveAt(0);

			//remove first character (e.g. H9 > 9, HK > 10, H0 > 10)
			char cardRank = card[1];
			cardToValueMap.TryGetValue(cardRank, out int cardvalue);
			cardToNameMap.TryGetValue(card, out string cardName);

			activeHand.Cards.Add(cardvalue.ToString());
			
			string totalHandValue = CalculateHandValue(activeHand.Cards);

			GameModel model = new GameModel
			{
				User_ID = player.User_ID,
				Action = activeHand.Cards.Count > 2 ? GameAction.HIT : GameAction.CARD_DRAWN,
				Card = cardName,
				Total_Card_Value = totalHandValue,
				Cards_In_Deck = group.Deck.Count,
				Hand = index + 1
			};

			await OnGameInfoToGroup?.Invoke(group, model);

			//end turn for player if above or equal to 21
			if (GetBestHandValue(totalHandValue) > 21 || GetBestHandValue(totalHandValue) == 21) 
			{
				activeHand.IsFinished = true;

				//no more hands left
				if (GetActiveHand(player).hand == null)
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
			}

			Console.WriteLine($"{player.User_ID} received {cardName}, value in hand: {CalculateHandValue(activeHand.Cards)}");
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
				Total_Card_Value = CalculateHandValue(group.DealerHand),
				Cards_In_Deck = group.Deck.Count,
				Hand = 1
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

			var (activeHand, index) = GetActiveHand(player);

			if (activeHand == null)
			{
				await OnGroupNotification?.Invoke(group, "An error occured, please try again later.", NotificationType.GAME, default);
				Console.WriteLine($"{player.User_ID} has no active hands to deal a card.");
				return;
			}

			GameModel model = new GameModel
			{
				User_ID = player.User_ID,
				Action = GameAction.STAND,
				Total_Card_Value = CalculateHandValue(activeHand.Cards),
				Hand = index + 1
			};

			//notify about game-action (stand)
			await OnGameInfoToGroup?.Invoke(group, model);

			activeHand.IsFinished = true;

			//no more hands left
			if (GetActiveHand(player).hand == null)
			{
				player.HasFinished = true;
				GameModel finishModel = new GameModel
				{
					User_ID = player.User_ID,
					Action = GameAction.PLAYER_FINISHED,
				};

				//notify about game-action (player finished playing)
				await OnGameInfoToGroup?.Invoke(group, finishModel);
			}
		}

		private async Task Double(Player player)
		{
			Group group = SharedData.GetGroupForPlayer(player);

			if (group.Status != Group.GroupStatus.PLAYING) return;

			var (activeHand, index) = GetActiveHand(player);

			if (activeHand == null)
			{
				await OnGroupNotification?.Invoke(group, "An error occured, please try again later.", NotificationType.GAME, default);
				Console.WriteLine($"{player.User_ID} has no active hands to deal a card.");
				return;
			}

			if (activeHand.Cards.Count != 2)
			{
				await OnNotification?.Invoke(player, "You can only double down on the first 2 cards.", NotificationType.TOAST, ToastType.WARNING);
				return;
			}

			group.Bets.TryGetValue(player, out int bet);

			if (player.Credits < bet)
			{
				await OnNotification?.Invoke(player, "You don't have enough credits to double down.", NotificationType.TOAST, ToastType.WARNING);
				return;
			}

			activeHand.IsDoubled = true;

			player.Credits -= bet;

			string card = group.Deck[0];
			group.Deck.RemoveAt(0);

			//remove first character (e.g. H9 > 9, HK > 10, H0 > 10)
			char cardRank = card[1];
			cardToValueMap.TryGetValue(cardRank, out int cardvalue);
			cardToNameMap.TryGetValue(card, out string cardName);

			activeHand.Cards.Add(cardvalue.ToString());

			string totalHandValue = CalculateHandValue(activeHand.Cards);

			GameModel model = new GameModel
			{
				User_ID = player.User_ID,
				Action = GameAction.DOUBLE,
				Card = cardName,
				Total_Card_Value = totalHandValue,
				Cards_In_Deck = group.Deck.Count,
				Credits = player.Credits,
				Bet = group.Bets[player] + bet,
				Total_Bet_Value = CalculateTotalBetValue(player),
				Hand = index + 1
			};

			await OnGameInfoToGroup?.Invoke(group, model);

			activeHand.IsFinished = true;

			//no more hands left
			if (GetActiveHand(player).hand == null)
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

			Console.WriteLine($"{player.User_ID} doubled down, betting {group.Bets[player]} and received {cardName}, value in hand: {CalculateHandValue(activeHand.Cards)}");
		}

		private async Task Surrender(Player player)
		{
			Group group = SharedData.GetGroupForPlayer(player);

			if (group.Status != Group.GroupStatus.PLAYING) return;

			var (activeHand, index) = GetActiveHand(player);

			if (activeHand == null)
			{
				await OnGroupNotification?.Invoke(group, "An error occured, please try again later.", NotificationType.GAME, default);
				Console.WriteLine($"{player.User_ID} has no active hands to deal a card.");
				return;
			}

			if (activeHand.Cards.Count != 2 || player.Hands.Count != 1) 
			{
				await OnNotification?.Invoke(player, "You can only surrender on the first 2 cards.", NotificationType.TOAST, ToastType.WARNING);
				return;
			}

			group.Bets.TryGetValue(player, out int bet);

			//give back half
			player.Credits += bet / 2;

			activeHand.Cards.Clear();

			GameModel model = new GameModel
			{
				User_ID = player.User_ID,
				Action = GameAction.SURRENDER,
				Total_Card_Value = "0",
				Credits = player.Credits,
				Hand = index + 1
			};

			await OnGameInfoToGroup?.Invoke(group, model);

			activeHand.IsFinished = true;

			player.HasFinished = true;
			GameModel finishedModel = new GameModel
			{
				User_ID = player.User_ID,
				Action = GameAction.PLAYER_FINISHED,
			};

			//notify about game-action (player finished playing)
			await OnGameInfoToGroup?.Invoke(group, finishedModel);

			Console.WriteLine($"{player.User_ID} surrendered.");
		}

		private async Task Insure(Player player)
		{
			Group group = SharedData.GetGroupForPlayer(player);

			if (group.Status != Group.GroupStatus.PLAYING) return;


			if (player.HasInsurance) 
			{
				await OnNotification?.Invoke(player, "You are already insured.", NotificationType.TOAST, ToastType.WARNING);
				return;
			}

			if (player.Hands.Count != 1 || player.Hands[0].Cards.Count != 2)
			{
				await OnNotification?.Invoke(player, "Insurance is only available with your initial two cards.", NotificationType.TOAST, ToastType.WARNING);
				return;
			}

			if (group.DealerHand[0] != "11") 
			{
				await OnNotification?.Invoke(player, "You can only take insurance when de dealer shows an Ace.", NotificationType.TOAST, ToastType.WARNING);
				return;
			}

			group.Bets.TryGetValue(player, out int bet);

			if (player.Credits >= bet / 2)
			{
				player.HasInsurance = true;
				player.Credits -= bet / 2;

				GameModel model = new GameModel
				{
					User_ID = player.User_ID,
					Action = GameAction.INSURE,
					Bet = bet / 2,
				};

				//notify about game-action (insure)
				await OnGameInfoToGroup?.Invoke(group, model);
			}
			else
			{
				await OnNotification?.Invoke(player, "You don't have enough credits for insurance.", NotificationType.TOAST, ToastType.WARNING);
			}
		}

		private async Task Split(Player player)
		{
			Group group = SharedData.GetGroupForPlayer(player);

			if (group.Status != Group.GroupStatus.PLAYING) return;

			var (activeHand, index) = GetActiveHand(player);

			if (activeHand == null)
			{
				await OnGroupNotification?.Invoke(group, "An error occured, please try again later.", NotificationType.GAME, default);
				Console.WriteLine($"{player.User_ID} has no active hands to deal a card.");
				return;
			}

			if (player.Hands.Count == 4) 
			{
				await OnNotification?.Invoke(player, "You can only split 3 times.", NotificationType.TOAST, ToastType.WARNING);
				return;
			}

			if (activeHand.Cards[0] != activeHand.Cards[1])
			{
				await OnNotification?.Invoke(player, "You can only split on identically ranked initial cards.", NotificationType.TOAST, ToastType.WARNING);
				return;
			}

			if (activeHand.Cards.Count != 2)
			{
				await OnNotification?.Invoke(player, "You can only split on the first 2 cards of a hand.", NotificationType.TOAST, ToastType.WARNING);
				return;
			}

			group.Bets.TryGetValue(player, out int bet);

			if (player.Credits < bet)
			{
				await OnNotification?.Invoke(player, "You don't have enough credits to split.", NotificationType.TOAST, ToastType.WARNING);
				return;
			}

			player.Credits -= bet;

			var hand1 = new Player.Hand();
			var hand2 = new Player.Hand();

			hand1.Cards.Add(activeHand.Cards[0]); 
			hand2.Cards.Add(activeHand.Cards[1]); 
			
			player.Hands.Remove(activeHand);

			player.Hands.Add(hand1);
			player.Hands.Add(hand2);

			Console.WriteLine($"Player {player.User_ID} has the following hands:");

			for (int i = 0; i < player.Hands.Count; i++)
			{
				var hand = player.Hands[i];
				var handValue = CalculateHandValue(hand.Cards); 

				Console.WriteLine($"Hand {i + 1}: {string.Join(", ", hand.Cards)} | Value: {handValue}");
			}

			GameModel model = new GameModel
			{
				User_ID = player.User_ID,
				Action = GameAction.SPLIT,
				Hand = index + 1,
				Total_Bet_Value = CalculateTotalBetValue(player),
				Bet = bet,
			};
		
			//notify about game-action (split)
			await OnGameInfoToGroup?.Invoke(group, model);
		}

		private async Task Bet(Player player, string bet_amount) 
		{
			if (!int.TryParse(bet_amount, out int bet) || bet % 10 != 0) { 
				await OnNotification?.Invoke(player, "Invalid bet value received", NotificationType.TOAST, ToastType.ERROR);
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
				Total_Bet_Value = bet,
			};

			await OnGameInfoToGroup?.Invoke(group, model);

			//all bets locked in? start game
			if (group.Bets.Count == group.Members.Count)
			{
				group.Status = Group.GroupStatus.PLAYING;
				StartGame(group);
			}
		}

		private int CalculateTotalBetValue(Player player) 
		{
			Group group = SharedData.GetGroupForPlayer(player);

			int totalBet = 0;

			foreach (var hand in player.Hands)
			{
				group.Bets.TryGetValue(player, out int bet);

				totalBet += hand.IsDoubled ? bet * 2 : bet;
			}

			return totalBet;
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