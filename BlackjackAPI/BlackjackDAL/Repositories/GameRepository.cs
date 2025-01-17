using BlackjackCommon.Entities.Friend;
using BlackjackCommon.Entities.History;
using BlackjackCommon.Entities.User;
using BlackjackLogic.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;
using System;

namespace BlackjackDAL.Repositories
{
    public class GameRepository : IGameRepository
    {
        private readonly AppDbContext _context;

        public GameRepository(AppDbContext context)
        {
            _context = context;
        }

        public void SaveEvent(int user_id, string group_id, string action, string? result, string payload, int round_number)
        {
            try
            {
                var parsedAction = Enum.Parse<HistoryAction>(action);
                var parsedResult = string.IsNullOrEmpty(result) ? (HistoryResult?)null : Enum.Parse<HistoryResult>(result);

                var historyEntry = new History
                {
                    history_group_id = group_id,
                    history_user_id = user_id,
                    history_action = parsedAction,
                    history_result = parsedResult,
                    history_payload = payload,
                    history_round_number = round_number == 0 ? 1 : round_number, //edge case for betting in a new game
                    history_datetime = DateTime.Now
                };

                _context.History.Add(historyEntry);

                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while saving the event: {ex.Message}");
                throw;
            }
        }



        public void SavePlaytime(int user_id, TimeSpan playtime)
        {
            try
            {
                var user = _context.User.FirstOrDefault(u => u.user_id == user_id);

                if (user == null)
                {
                    Console.WriteLine($"User with ID {user_id} not found.");
                    return;
                }

                user.user_total_playtime = (user.user_total_playtime ?? TimeSpan.Zero) + playtime;

                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while saving the event: {ex.Message}");
                throw;
            }
        }

    }
}
