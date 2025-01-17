using BlackjackLogic.Interfaces.Logic;
using BlackjackLogic.Interfaces.Repository;
using BlackjackCommon.Models;
using BlackjackCommon.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;


namespace BlackjackLogic
{
    public class ReplayLogic : IReplayLogic
    {
        private readonly IReplayRepository _replayDAL;
        public ReplayLogic(IReplayRepository replayDAL)
        {
            _replayDAL = replayDAL;

        }

        public async Task<Response<List<ReplayModel>>> RetrieveReplayAsync(int user_id, string group_id)
        {
            try
            {
                //retrieve rounds the player was active in
                var rounds = await _replayDAL.RetrieveGameRoundsAsync(user_id, group_id);

                if (rounds == null || rounds.Count == 0)
                {
                    return new Response<List<ReplayModel>>(null, "Default");
                }

                //retrieve lobby members
                var lobby = await _replayDAL.RetrieveLobbyMembersAsync(rounds, user_id, group_id);

                if (lobby == null || lobby.Count == 0)
                {
                    return new Response<List<ReplayModel>>(null, "Default");
                }

                //retrieve game per round
                var gamereplay = await _replayDAL.RetrieveGameReplayAsync(rounds, group_id);

                if (gamereplay == null || gamereplay.Count == 0)
                {
                    return new Response<List<ReplayModel>>(null, "Default");
                }

                var combinedData = new List<ReplayModel>();

                combinedData.AddRange(lobby);
                combinedData.AddRange(gamereplay);

                //retrieve chat 
                var chatmessages = await _replayDAL.RetrieveChatReplayAsync(rounds, group_id);

                if (chatmessages != null && chatmessages.Count > 0)
                {
                    combinedData.AddRange(chatmessages);
                }

                var sortedData = combinedData
                    .OrderBy(item => item.round)
                    .ThenBy(item => item.datetime)
                    .ToList();

                // Return the combined data
                return new Response<List<ReplayModel>>(sortedData, "Success");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw;
            }
        }

        public async Task<Response<List<ReplayListModel>>> RetrieveReplayListAsync(int user_id)
        {
            try
            {
                //retrieve groups the player was active in
                var groupData = await _replayDAL.RetrieveGroupIdsAsync(user_id);

                if (groupData == null || groupData.Count == 0)
                {
                    return new Response<List<ReplayListModel>>(null, "NoGroupsFound");
                }

                List<ReplayListModel> replayList = new List<ReplayListModel>();

                foreach (var group in groupData)
                {

                    string group_id = group.Item1;
                    DateTime latestDateTime = group.Item2;

                    //retrieve rounds the player was active in
                    var rounds = await _replayDAL.RetrieveGameRoundsAsync(user_id, group_id);

                    if (rounds == null || rounds.Count == 0)
                    {
                        continue;
                    }

                    int totalRounds = rounds.Count;
                    int wins = 0;
                    int losses = 0;
                    int earningsAmt = 0;
                    int lossesAmt = 0;

                    foreach (int round in rounds)
                    {
                        //retrieve actions for the current round and group_id
                        var gamereplay = await _replayDAL.RetrieveGameReplayAsync(new List<int> { round }, group_id);

                        if (gamereplay == null || gamereplay.Count == 0)
                        {
                            continue;
                        }

                        foreach (var action in gamereplay)
                        {
                            var gameModel = JsonConvert.DeserializeObject<GameModel>(action.payload);
                            if (gameModel == null || (gameModel.Action != GameAction.GAME_FINISHED && gameModel.Action != GameAction.INSURE && gameModel.Action != GameAction.INSURANCE_PAID) || gameModel.User_ID != user_id)
                            {
                                continue;
                            }

                            if (gameModel.Action == GameAction.INSURE)
                            {
                                lossesAmt += gameModel.Bet ?? 0;
                                continue;
                            }

                            if (gameModel.Action == GameAction.INSURANCE_PAID)
                            {
                                earningsAmt += gameModel.Bet ?? 0;
                                continue;
                            }

                            var result = gameModel.Result;
                            var betAmount = gameModel.Bet ?? 0;

                            if (result == GameResult.WIN || result == GameResult.BLACKJACK)
                            {
                                wins++;
                                earningsAmt += betAmount;
                            }
                            else if (result == GameResult.LOSE || result == GameResult.BUSTED || result == GameResult.SURRENDER)
                            {
                                losses++;
                                lossesAmt += betAmount;
                            }
                        }
                    }

                    var replayModel = new ReplayListModel
                    {
                        group_id = group_id,
                        round = totalRounds,
                        datetime = latestDateTime,
                        wins = wins,
                        losses = losses,
                        earnings_amt = earningsAmt,
                        losses_amt = lossesAmt
                    };

                    replayList.Add(replayModel);
                }

                return new Response<List<ReplayListModel>>(replayList, "Success");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw;
            }
        }


    }
}