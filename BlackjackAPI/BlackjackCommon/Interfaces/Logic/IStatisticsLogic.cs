using BlackjackCommon.Models;
using BlackjackCommon.ViewModels;

namespace BlackjackCommon.Interfaces.Logic
{
	public interface IStatisticsLogic
	{
		Task<Response<StatisticsModel>> RetrieveStatisticsAsync(int user_id);
		Task<Response<List<LeaderboardModel>>> RetrieveLeaderboardAsync();

	}
}
