using BlackjackCommon.Entities.History;
using BlackjackCommon.Interfaces.Repository;
using BlackjackCommon.ViewModels;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Globalization;

namespace BlackjackDAL.Repositories
{
    public class ReplayRepository : IReplayRepository
    {
        private readonly AppDbContext _context;

        public ReplayRepository(AppDbContext context)
        {
            _context = context;
        }


        public async Task<List<ReplayModel>> RetrieveLobbyMembersAsync(List<int> rounds, int user_id, string group_id)
        {
            try
            {
				var gameActions = new List<ReplayModel>();

				foreach (var round in rounds)
				{
					var playerIds = await _context.History
                    .Where(h => h.history_group_id == group_id && h.history_round_number == round && h.history_action == HistoryAction.BET_PLACED)
                    .Select(h => h.history_user_id)
                    .Distinct()
                    .ToListAsync();

                    //sort list based on who received their card first.
					var playerIdsInCardDrawOrder = await _context.History
	                    .Where(h =>
		                    h.history_group_id == group_id &&
		                    h.history_round_number == round &&
		                    h.history_action == HistoryAction.CARD_DRAWN &&
		                    playerIds.Contains(h.history_user_id)) 			                       
	                    .OrderBy(h => h.history_datetime)
	                    .Select(h => h.history_user_id)
	                    .Distinct() 
	                    .ToListAsync();

                    //placed a bet but left before receiving a card? place user at bottom of group
					var playersWhoHaventDrawnCard = playerIds.Except(playerIdsInCardDrawOrder).ToList();
					var combinedPlayerIdsInOrder = playerIdsInCardDrawOrder.Concat(playersWhoHaventDrawnCard).ToList();

					var members = await _context.User
                        .Where(u => combinedPlayerIdsInOrder.Contains(u.user_id))
                        .Select(u => new Member(
                            u.user_id,
                            u.user_name,
                            false,
                            false,
                            u.user_id == user_id ? 0 : (int?)null
                        ))
                        .ToListAsync();

                    //order based on cardreceiving order
					var orderedMembers = members
	                    .OrderBy(m => playerIdsInCardDrawOrder.IndexOf(m.User_ID))  
	                    .ToList();

					var groupModal = new GroupModel
                    {
                        Group_ID = group_id.Substring(0, 6),
                        Members = orderedMembers
					};

                
                    gameActions.Add(new ReplayModel
                    {
                        type = "LOBBY",
                        payload = JsonConvert.SerializeObject(groupModal),
                        round = round,
                        datetime = DateTime.MinValue
                    });
                }

                return gameActions;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw;
            }
        }

        public async Task<List<ReplayModel>> RetrieveGameReplayAsync(List<int> rounds, string group_id)
        {
            try
            {
                var gameActions = _context.History
                    .Where(h => rounds.Contains(h.history_round_number) && h.history_group_id == group_id && h.history_action != HistoryAction.CREDITS_UPDATE)
					.OrderBy(h => h.history_datetime)
                    .ToList();

                if (gameActions == null || gameActions.Count == 0)
                    return null;

                return gameActions.Select(h => new ReplayModel
                {
                    type = "GAME",
                    payload = h.history_payload,
                    round = h.history_round_number,
                    datetime = h.history_datetime
                }).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw;
            }
        }

        public async Task<List<ReplayModel>> RetrieveChatReplayAsync(List<int> rounds, string group_id)
        {
            try
            {
                var chatMessages = new List<ReplayModel>();

                //get msg per round between 2 timestamps
                foreach (var round in rounds)
                {
                    //get first and last timestamp
                    var roundTimestamps = await _context.History
                        .Where(m => m.history_group_id == group_id && m.history_round_number == round)
                        .OrderBy(m => m.history_datetime)
                        .Select(m => m.history_datetime)
                        .ToListAsync();

                    if (roundTimestamps.Count == 0)
                        continue;

                    //get start/end based on rounds
                    var roundStartTimestamp = roundTimestamps.First();
                    var roundEndTimestamp = roundTimestamps.Last();

                    var roundMessages = await (from m in _context.Message
                        join uSender in _context.User on m.message_sender equals uSender.user_id
                        where m.message_group == group_id
                            && m.message_datetime >= roundStartTimestamp
                            && m.message_datetime <= roundEndTimestamp
                        orderby m.message_datetime ascending
                        select new MessageModel
                        {
                            Type = MessageType.GROUP,
                            SenderName = uSender.user_name,
                            Sender = m.message_sender,
                            Receiver = 0,
                            Message = m.message_deleted ? "This message has been deleted." : m.message_content,
                            Datetime = m.message_datetime
                        }).ToListAsync();


                    //none in this round, skip to next
                    if (roundMessages == null || !roundMessages.Any())
                        continue;

                    chatMessages.AddRange(roundMessages.Select(m => new ReplayModel
                    {
                        type = "MESSAGE",
                        payload = JsonConvert.SerializeObject(m),
                        round = round,
                        datetime = m.Datetime
                    }));
                }

                if (chatMessages.Count == 0)
                    return null;

                return chatMessages;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw;
            }
        }

        public async Task<List<int>> RetrieveGameRoundsAsync(int user_id, string group_id)
        {
            try
            {
                var rounds = await _context.History
                    .Where(h => h.history_user_id == user_id
                            && h.history_group_id == group_id
                            && h.history_action == HistoryAction.BET_PLACED)
                    .Select(h => h.history_round_number)
                    .Distinct()
                    .OrderBy(r => r)
                    .ToListAsync();

                return rounds;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw;
            }
        }
    }
}
