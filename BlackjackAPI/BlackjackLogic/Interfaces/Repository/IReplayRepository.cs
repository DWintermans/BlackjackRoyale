using BlackjackCommon.ViewModels;

namespace BlackjackLogic.Interfaces.Repository
{
    public interface IReplayRepository
    {
        Task<List<ReplayModel>> RetrieveGameReplayAsync(List<int> rounds, string group_id);
        Task<List<int>> RetrieveGameRoundsAsync(int user_id, string group_id);
        Task<List<ReplayModel>> RetrieveChatReplayAsync(List<int> rounds, string group_id);
        Task<List<ReplayModel>> RetrieveLobbyMembersAsync(List<int> rounds, int user_id, string group_id);
        Task<List<Tuple<string, DateTime>>> RetrieveGroupIdsAsync(int user_id);


    }
}
