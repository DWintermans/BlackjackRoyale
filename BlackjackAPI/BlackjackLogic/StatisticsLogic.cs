using BlackjackCommon.Interfaces.Logic;
using BlackjackCommon.Interfaces.Repository;
using BlackjackCommon.Models;
using BlackjackCommon.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BlackjackLogic
{
    public class StatisticsLogic : IStatisticsLogic
    {
        private readonly IStatisticsRepository _statisticsDAL;
        public StatisticsLogic(IStatisticsRepository statisticsDAL)
        {
            _statisticsDAL = statisticsDAL;

        }

        public async Task<Response<StatisticsModel>> RetrieveStatisticsAsync(int user_id)
        {
            try
            {
                var data = await _statisticsDAL.RetrieveStatisticsAsync(user_id);

                if (data == null)
                {
                    return new Response<StatisticsModel>(null, "Default");
                }

                if (Int32.Parse(data.balance) >= 1000)
                {
                    data.balance = (Int32.Parse(data.balance) / 1000.0).ToString("0.##") + "K";
                }

                return new Response<StatisticsModel>(data, "Success");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw;
            }
        }

        public async Task<Response<List<LeaderboardModel>>> RetrieveLeaderboardAsync()
        {
            try
            {
                var data = await _statisticsDAL.RetrieveLeaderboardAsync();

                if (data == null)
                {
                    return new Response<List<LeaderboardModel>>(null, "Default");
                }

                return new Response<List<LeaderboardModel>>(data, "Success");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw;
            }
        }



    }
}