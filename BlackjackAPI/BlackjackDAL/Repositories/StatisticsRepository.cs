using BlackjackCommon.Entities.History;
using BlackjackCommon.Entities.User;
using BlackjackCommon.Interfaces.Repository;
using BlackjackCommon.ViewModels;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Globalization;

namespace BlackjackDAL.Repositories
{
    public class StatisticsRepository : IStatisticsRepository
    {
        private readonly AppDbContext _context;

        public StatisticsRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<StatisticsModel?> RetrieveStatisticsAsync(int user_id)
        {
            try
            {
                var user = await _context.User.FirstOrDefaultAsync(u => u.user_id == user_id);

                if (user == null)
                {
                    return null;
                }

                var gamesPlayed = await _context.History
                    .Where(h => h.history_user_id == user_id && h.history_action == HistoryAction.BET_PLACED)
                    .CountAsync();

                var totalGameWins = await _context.History
                    .Where(h => h.history_user_id == user_id && h.history_action == HistoryAction.GAME_FINISHED && (h.history_result == HistoryResult.WIN || h.history_result == HistoryResult.BLACKJACK))
                    .CountAsync();

                var totalGameLosses = await _context.History
                    .Where(h => h.history_user_id == user_id && h.history_action == HistoryAction.GAME_FINISHED && (h.history_result == HistoryResult.LOSE || h.history_result == HistoryResult.BUSTED))
                    .CountAsync();

                var blackjackAchieved = await _context.History
                    .Where(h => h.history_user_id == user_id && h.history_action == HistoryAction.GAME_FINISHED && h.history_result == HistoryResult.BLACKJACK)
                    .CountAsync();

                var tied = await _context.History
                    .Where(h => h.history_user_id == user_id && h.history_action == HistoryAction.GAME_FINISHED && h.history_result == HistoryResult.PUSH)
                    .CountAsync();

                var surrendered = await _context.History
                    .Where(h => h.history_user_id == user_id && h.history_action == HistoryAction.SURRENDER)
                    .CountAsync();

                var split = await _context.History
                    .Where(h => h.history_user_id == user_id && h.history_action == HistoryAction.SPLIT)
                    .CountAsync();

                var doubled = await _context.History
                    .Where(h => h.history_user_id == user_id && h.history_action == HistoryAction.DOUBLE)
                    .CountAsync();

                var usedInsurance = await _context.History
                    .Where(h => h.history_user_id == user_id && h.history_action == HistoryAction.INSURE)
                    .CountAsync();

                var receivedInsurance = await _context.History
                    .Where(h => h.history_user_id == user_id && h.history_action == HistoryAction.INSURANCE_PAID)
                    .CountAsync();

                var longestWinningStreak = 0;
                var longestLosingStreak = 0;
                var highestGameWin = 0;
                var highestGameLoss = 0;

                int currentWinningStreak = 0;
                int currentLosingStreak = 0;

                var userHistory = await _context.History
                    .Where(h => h.history_user_id == user_id && h.history_action == HistoryAction.GAME_FINISHED)
                    .OrderBy(h => h.history_datetime)
                    .ToListAsync();

                foreach (var game in userHistory)
                {
                    var historyPayload = JsonConvert.DeserializeObject<Dictionary<string, object>>(game.history_payload);

                    if (historyPayload != null && historyPayload.ContainsKey("Bet"))
                    {
                        int betAmount = Convert.ToInt32(historyPayload["Bet"]);

                        //count push as win for streak
                        if (game.history_result == HistoryResult.WIN || game.history_result == HistoryResult.BLACKJACK || game.history_result == HistoryResult.PUSH)
                        {
                            currentWinningStreak++;
                            currentLosingStreak = 0;

                            if (betAmount > highestGameWin)
                            {
                                highestGameWin = betAmount;
                            }
                        }
                        else if (game.history_result == HistoryResult.LOSE || game.history_result == HistoryResult.BUSTED || game.history_result == HistoryResult.SURRENDER)
                        {
                            currentLosingStreak++;
                            currentWinningStreak = 0;

                            if (betAmount > highestGameLoss)
                            {
                                highestGameLoss = betAmount;
                            }
                        }
                    }

                    if (currentWinningStreak > longestWinningStreak)
                    {
                        longestWinningStreak = currentWinningStreak;
                    }
                    if (currentLosingStreak > longestLosingStreak)
                    {
                        longestLosingStreak = currentLosingStreak;
                    }
                }

                var statistics = new StatisticsModel
                {
                    balance = user.user_balance.ToString(),
                    total_earnings = user.user_total_earnings_amt,
                    total_losses = user.user_total_losses_amt,
                    playtime = user.user_total_playtime.ToString(),

                    games_played = gamesPlayed,
                    total_game_wins = totalGameWins,
                    total_game_losses = totalGameLosses,
                    blackjack_achieved = blackjackAchieved,
                    tied = tied,
                    surrendered = surrendered,
                    split = split,
                    doubled = doubled,
                    used_insurance = usedInsurance,
                    received_insurance = receivedInsurance,

                    longest_winning_streak = longestWinningStreak,
                    longest_losing_streak = longestLosingStreak,
                    highest_game_win = highestGameWin,
                    highest_game_loss = highestGameLoss
                };

                return statistics;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw;
            }
        }

        public async Task<List<LeaderboardModel>> RetrieveLeaderboardAsync()
        {
            try
            {
                var leaderboard = await _context.User
                    .Where(u => u.user_status == UserStatus.active &&
                                (u.user_total_earnings_amt > 0 || u.user_total_losses_amt > 0))
                    .Select(u => new
                    {
                        u.user_name,
                        u.user_id,
                        u.user_total_earnings_amt,
                        u.user_total_losses_amt,
                        gamesPlayed = _context.History
                            .Where(h => h.history_user_id == u.user_id && h.history_action == HistoryAction.BET_PLACED)
                            .Count()
                    })
                    .ToListAsync();

                var leaderboardModels = leaderboard
                    .Where(u => u.gamesPlayed >= 10)
                    .Select(user =>
                    {
                        int totalEarnings = user.user_total_earnings_amt ?? 0;
                        int totalLosses = user.user_total_losses_amt ?? 0;
                        double ratio = totalLosses == 0 ? totalEarnings : (double)totalEarnings / totalLosses;
                        return new LeaderboardModel
                        {
                            user_name = user.user_name,
                            ratio = ratio.ToString("F1")
                        };
                    })
                    .OrderByDescending(l => double.Parse(l.ratio))
                    .Take(10)
                    .ToList();

                return leaderboardModels;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw;
            }
        }


    }
}
