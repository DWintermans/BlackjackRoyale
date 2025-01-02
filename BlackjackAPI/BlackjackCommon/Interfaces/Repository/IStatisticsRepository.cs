using BlackjackCommon.ViewModels;

namespace BlackjackCommon.Interfaces.Repository
{
    public interface IStatisticsRepository
    {
		Task<StatisticsModel> RetrieveStatisticsAsync(int user_id);
		Task<List<LeaderboardModel>> RetrieveLeaderboardAsync();
	}
}
