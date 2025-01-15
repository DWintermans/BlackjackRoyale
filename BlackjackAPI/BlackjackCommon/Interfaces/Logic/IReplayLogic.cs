using BlackjackCommon.Models;
using BlackjackCommon.ViewModels;

namespace BlackjackCommon.Interfaces.Logic
{
    public interface IReplayLogic
    {
        Task<Response<List<ReplayModel>>> RetrieveReplayAsync(int user_id, string group_id);
        Task<Response<List<ReplayListModel>>> RetrieveReplayListAsync(int user_id);
    }
}
